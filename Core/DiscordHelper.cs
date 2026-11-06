using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MoliTweak.Core;

public class DiscordProfile
{
    public string Username { get; set; } = "Discord User";
    public string DisplayName { get; set; } = "";
    public string Id { get; set; } = "";
    public string? AvatarHash { get; set; }
    public string? AvatarUrl => !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(AvatarHash)
        ? $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.png?size=128"
        : null;
    public string? LocalAvatarPath { get; set; }
}

public static class DiscordHelper
{
    private static DiscordProfile? _cached;
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(6) };

    public static async Task<DiscordProfile> GetProfileAsync()
    {
        if (_cached != null) return _cached;

        // 1. Canlı Discord IPC Named Pipe üzerinden gerçek zamanlı aktif kullanıcıyı ve güncel avatarı al
        var profile = await FetchFromIpcAsync() ?? ScanLocalDiscord() ?? new DiscordProfile
        {
            Username = Environment.UserName,
            Id = "0"
        };

        // 2. Güncel avatarı CDN'den indir ve yerel olarak önbelleğe al
        if (!string.IsNullOrEmpty(profile.AvatarUrl))
        {
            try
            {
                var cacheDir = Path.Combine(Path.GetTempPath(), "lemonistan");
                Directory.CreateDirectory(cacheDir);
                var localPath = Path.Combine(cacheDir, $"avatar_{profile.Id}_{profile.AvatarHash}.png");

                if (!File.Exists(localPath) || new FileInfo(localPath).Length == 0)
                {
                    var bytes = await _httpClient.GetByteArrayAsync(profile.AvatarUrl);
                    await File.WriteAllBytesAsync(localPath, bytes);
                }

                profile.LocalAvatarPath = localPath;
            }
            catch { }
        }

        _cached = profile;
        return _cached;
    }

    public static DiscordProfile GetProfile()
    {
        return GetProfileAsync().GetAwaiter().GetResult();
    }

    private static async Task<DiscordProfile?> FetchFromIpcAsync()
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                using var pipe = new NamedPipeClientStream(".", $"discord-ipc-{i}", PipeDirection.InOut);
                await pipe.ConnectAsync(800);

                // Standart Discord RPC Handshake (VS Code Client ID: 383226320970055681)
                var payload = Encoding.UTF8.GetBytes("{\"v\":1,\"client_id\":\"383226320970055681\"}");

                using var writer = new BinaryWriter(pipe, Encoding.UTF8, leaveOpen: true);
                writer.Write(0); // Opcode 0 = Handshake
                writer.Write(payload.Length);
                writer.Write(payload);
                writer.Flush();

                using var reader = new BinaryReader(pipe, Encoding.UTF8, leaveOpen: true);
                int op = reader.ReadInt32();
                int len = reader.ReadInt32();
                if (len <= 0 || len > 65536) continue;

                byte[] respBytes = reader.ReadBytes(len);
                var json = Encoding.UTF8.GetString(respBytes);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("data", out var data) && data.TryGetProperty("user", out var user))
                {
                    string id = user.GetProperty("id").GetString() ?? "";
                    string username = user.GetProperty("username").GetString() ?? "Discord User";
                    string displayName = user.TryGetProperty("global_name", out var gn) && gn.ValueKind == JsonValueKind.String
                        ? (gn.GetString() ?? username)
                        : username;
                    string? avatar = user.TryGetProperty("avatar", out var av) && av.ValueKind == JsonValueKind.String
                        ? av.GetString()
                        : null;

                    return new DiscordProfile
                    {
                        Id = id,
                        Username = username,
                        DisplayName = displayName,
                        AvatarHash = avatar
                    };
                }
            }
            catch { }
        }

        return null;
    }

    private static DiscordProfile? ScanLocalDiscord()
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var levelDbPath = Path.Combine(appData, "discord", "Local Storage", "leveldb");
            if (!Directory.Exists(levelDbPath)) return null;

            var files = Directory.GetFiles(levelDbPath, "*.ldb")
                .Concat(Directory.GetFiles(levelDbPath, "*.log"))
                .ToArray();

            Array.Sort(files, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

            var rx1 = new Regex("\"users\":\\[\\{\"id\":\"(?<id>\\d+)\",\"avatar\":\"(?<avatar>[^\"]*)\",\"username\":\"(?<user>[^\"]+)\"", RegexOptions.Compiled);
            var rxUser = new Regex("\"username\":\"(?<user>[a-zA-Z0-9_.\\-]+)\"", RegexOptions.Compiled);
            var rxId = new Regex("\"id\":\"(?<id>\\d{17,20})\"", RegexOptions.Compiled);
            var rxAvatar = new Regex("\"avatar\":\"(?<avatar>[a-f0-9]{32})\"", RegexOptions.Compiled);

            foreach (var file in files)
            {
                try
                {
                    var bytes = File.ReadAllBytes(file);
                    var text = Encoding.UTF8.GetString(bytes);

                    var m1 = rx1.Match(text);
                    if (m1.Success)
                    {
                        return new DiscordProfile
                        {
                            Id = m1.Groups["id"].Value,
                            AvatarHash = m1.Groups["avatar"].Value,
                            Username = m1.Groups["user"].Value
                        };
                    }

                    var mUser = rxUser.Match(text);
                    var mId = rxId.Match(text);
                    if (mUser.Success && mId.Success)
                    {
                        var mAv = rxAvatar.Match(text);
                        return new DiscordProfile
                        {
                            Id = mId.Groups["id"].Value,
                            Username = mUser.Groups["user"].Value,
                            AvatarHash = mAv.Success ? mAv.Groups["avatar"].Value : null
                        };
                    }
                }
                catch { }
            }
        }
        catch { }

        return null;
    }
}
