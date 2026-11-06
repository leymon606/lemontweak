using System.Net.Http;

namespace MoliTweak.Core;

public record UpdateInfo(bool Available, string CurrentVersion, string LatestVersion, string? DownloadUrl);

public sealed class UpdateChecker
{
    public static UpdateChecker Instance { get; } = new();
    public const string CurrentVersion = "1.0.0";

    private const string VersionUrl = "https://raw.githubusercontent.com/molitweak/releases/main/version.txt";

    public async Task<UpdateInfo> CheckAsync()
    {
        Logger.Instance.Info("Güncelleme kontrol ediliyor...");
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            var response = await client.GetStringAsync(VersionUrl);
            var lines = response.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var latestVersion = lines[0].Trim();
            var downloadUrl = lines.Length > 1 ? lines[1].Trim() : null;

            var current = new Version(CurrentVersion);
            var latest = new Version(latestVersion);

            bool available = latest > current;

            if (available)
                Logger.Instance.Warning($"Yeni sürüm mevcut: v{latestVersion}");
            else
                Logger.Instance.Success($"lemonistan güncel (v{CurrentVersion}).");

            return new UpdateInfo(available, CurrentVersion, latestVersion, downloadUrl);
        }
        catch
        {
            Logger.Instance.Success($"lemonistan güncel (v{CurrentVersion}).");
            return new UpdateInfo(false, CurrentVersion, CurrentVersion, null);
        }
    }
}
