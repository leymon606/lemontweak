using System.IO;

namespace MoliTweak.Core;

public sealed class TextureLossFix
{
    public static TextureLossFix Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    public void ApplyTextureLossFix(int vramBudgetMb = 3072)
    {
        Log.Info("Doku Kaybı & Görünmez Yol düzeltmesi uygulanıyor...");
        var fivemDir = Utils.GetFiveMDirectory();
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var cmdContent = "-nothreadaffinity\n-noInGameDepthOfField\n-shadowSoftness 0";

        var targets = new List<string>();
        if (fivemDir != null)
        {
            targets.Add(Path.Combine(fivemDir, "commandline.txt"));
            targets.Add(Path.Combine(Path.GetDirectoryName(fivemDir)!, "commandline.txt"));
        }
        targets.Add(Path.Combine(userProfile, "Documents", "Rockstar Games", "GTA V", "commandline.txt"));

        foreach (var t in targets)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(t)!);
                File.WriteAllText(t, cmdContent);
            }
            catch { }
        }

        if (fivemDir != null)
        {
            var iniPath = Path.Combine(fivemDir, "CitizenFX.ini");
            if (!File.Exists(iniPath))
                iniPath = Path.Combine(Path.GetDirectoryName(fivemDir)!, "CitizenFX.ini");

            try
            {
                var kv = new Dictionary<string, string>();
                if (File.Exists(iniPath))
                {
                    foreach (var line in File.ReadAllLines(iniPath))
                    {
                        var trimmed = line.Trim();
                        if (trimmed.Contains('='))
                        {
                            var parts = trimmed.Split('=', 2);
                            kv[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }

                kv["DisableNVSPCaps"] = "true";
                kv["streaming_memory_budget"] = vramBudgetMb.ToString();
                kv["DisableHyperspace"] = "true";

                var lines = new List<string>();
                foreach (var kvp in kv) lines.Add($"{kvp.Key}={kvp.Value}");
                File.WriteAllLines(iniPath, lines);
            }
            catch (Exception ex)
            {
                Log.Warning($"CitizenFX.ini güncellenirken hata: {ex.Message}");
            }
        }

        Log.Success("Doku Kaybı düzeltmesi uygulandı (çökme yapmayan güvenli mod).");
    }

    public void RevertTextureFix()
    {
        Log.Info("Texture Loss ayarları kaldırılıyor...");
        var fivemDir = Utils.GetFiveMDirectory();
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var targets = new List<string>
        {
            Path.Combine(userProfile, "Documents", "Rockstar Games", "GTA V", "commandline.txt")
        };
        if (fivemDir != null)
        {
            targets.Add(Path.Combine(fivemDir, "commandline.txt"));
            targets.Add(Path.Combine(Path.GetDirectoryName(fivemDir)!, "commandline.txt"));
        }

        foreach (var t in targets)
        {
            try { if (File.Exists(t)) File.Delete(t); }
            catch { }
        }

        Log.Success("Doku kaybı ayarları varsayılana döndürüldü.");
    }
}
