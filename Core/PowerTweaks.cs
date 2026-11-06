using Microsoft.Win32;

namespace MoliTweak.Core;

public sealed class PowerTweaks
{
    public static PowerTweaks Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    public void EnableUltimatePerformance()
    {
        Log.Info("Nihai Performans güç planı kontrol ediliyor...");
        Utils.RunCommand("powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");

        var (success, output, _) = Utils.RunCommand("powercfg /list");
        string? targetGuid = null;

        foreach (var line in output.Split('\n'))
        {
            var lower = line.ToLowerInvariant();
            if (lower.Contains("e9a42b02") || lower.Contains("ultimate performance") || lower.Contains("nihai performans"))
            {
                targetGuid = ExtractGuid(line);
                if (targetGuid != null) break;
            }
        }

        targetGuid ??= FindGuidByKeyword(output, "high performance", "yüksek performans", "8c5e7fda");

        if (targetGuid != null)
        {
            Utils.RunCommand($"powercfg /setactive {targetGuid}");
            Log.Success("Nihai Performans güç planı aktif edildi.");
        }
        else
        {
            Log.Info("Yüksek Performans güç planı aktif edildi.");
        }
    }

    public void DisableCpuCoreParking()
    {
        Log.Info("CPU Core Parking devre dışı bırakılıyor...");
        const string path = @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60f740d00\0cc5b647-c1df-4637-891a-dec60c185b7a";
        Utils.SetRegistryValue(Registry.LocalMachine, path, "ValueMin", RegistryValueKind.DWord, 100);
        Utils.SetRegistryValue(Registry.LocalMachine, path, "ValueMax", RegistryValueKind.DWord, 100);
        Utils.RunCommand("powercfg -setacvalueindex scheme_current sub_processor CPMINCORES 100");
        Utils.RunCommand("powercfg -setactive scheme_current");
        Log.Success("CPU Core Parking kapatıldı.");
    }

    public void ApplyAll()
    {
        EnableUltimatePerformance();
        DisableCpuCoreParking();
        Log.Success("Güç planı ve CPU optimizasyonları tamamlandı.");
    }

    private static string? ExtractGuid(string line)
    {
        foreach (var part in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var clean = part.Trim();
            if (clean.Length == 36 && clean.Count(c => c == '-') == 4) return clean;
        }
        return null;
    }

    private static string? FindGuidByKeyword(string output, params string[] keywords)
    {
        foreach (var line in output.Split('\n'))
        {
            var lower = line.ToLowerInvariant();
            if (keywords.Any(k => lower.Contains(k)))
                return ExtractGuid(line);
        }
        return null;
    }
}
