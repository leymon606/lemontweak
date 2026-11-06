using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Win32;

namespace MoliTweak.Core;

public sealed class FiveMTweaks
{
    public static FiveMTweaks Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    public static readonly string[] FiveMProcesses =
    [
        "FiveM.exe", "FiveM_GTAProcess.exe",
        "FiveM_b2060_GTAProcess.exe", "FiveM_b2189_GTAProcess.exe",
        "FiveM_b2372_GTAProcess.exe", "FiveM_b2545_GTAProcess.exe",
        "FiveM_b2699_GTAProcess.exe", "FiveM_b2802_GTAProcess.exe",
        "FiveM_b2944_GTAProcess.exe", "FiveM_b3095_GTAProcess.exe",
        "FiveM_b3258_GTAProcess.exe"
    ];

    public bool ApplyHighCpuPriority()
    {
        Log.Info("FiveM Yüksek CPU Önceliği ayarlanıyor...");
        const string baseIfeo = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
        int success = 0;

        foreach (var proc in FiveMProcesses)
        {
            var perfPath = $@"{baseIfeo}\{proc}\PerfOptions";
            var r1 = Utils.SetRegistryValue(Registry.LocalMachine, perfPath, "CpuPriorityClass", RegistryValueKind.DWord, 3);
            var r2 = Utils.SetRegistryValue(Registry.LocalMachine, perfPath, "IoPriority", RegistryValueKind.DWord, 3);
            Utils.SetRegistryValue(Registry.LocalMachine, perfPath, "PagePriority", RegistryValueKind.DWord, 5);
            if (r1 && r2) success++;
        }

        Log.Success($"FiveM Yüksek CPU Önceliği ayarlandı ({success}/{FiveMProcesses.Length} işlem).");
        return success > 0;
    }

    public bool OptimizeCitizenFxIni()
    {
        Log.Info("CitizenFX.ini yapılandırması kontrol ediliyor...");
        var fivemDir = Utils.GetFiveMDirectory();
        if (fivemDir == null) { Log.Warning("FiveM dizini bulunamadı."); return false; }

        var iniPath = Path.Combine(fivemDir, "CitizenFX.ini");
        if (!File.Exists(iniPath))
        {
            var alt = Path.Combine(Path.GetDirectoryName(fivemDir)!, "CitizenFX.ini");
            if (File.Exists(alt)) iniPath = alt;
        }

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
            kv["DisableHyperspace"] = "true";

            var lines = new List<string>();
            foreach (var kvp in kv) lines.Add($"{kvp.Key}={kvp.Value}");
            File.WriteAllLines(iniPath, lines);

            Log.Success("CitizenFX.ini başarıyla optimize edildi.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning($"CitizenFX.ini güncellenirken hata: {ex.Message}");
            return false;
        }
    }

    public bool ApplyGtaPreset(string presetType)
    {
        Log.Info($"GTA V grafik ayar preseti uygulanıyor: [{presetType}]...");
        var settingsPath = Utils.GetGtaSettingsFile();
        if (settingsPath == null || !File.Exists(settingsPath))
        {
            Log.Warning("GTA V settings.xml dosyası bulunamadı.");
            return false;
        }

        RestoreManager.Instance.BackupGtaSettings();

        try
        {
            var doc = XDocument.Load(settingsPath);
            var graphics = doc.Root?.Element("graphics");
            if (graphics == null) { Log.Warning("settings.xml formatı uyumsuz."); return false; }

            void SetVal(string tag, string val)
            {
                var elem = graphics.Element(tag);
                if (elem != null) elem.SetAttributeValue("value", val);
            }

            if (presetType == "ultra_fps")
            {
                SetVal("ShadowQuality", "0"); SetVal("ReflectionQuality", "0");
                SetVal("WaterQuality", "0"); SetVal("ParticleQuality", "0");
                SetVal("GrassQuality", "0"); SetVal("ShaderQuality", "0");
                SetVal("PostFX", "0"); SetVal("CityDensity", "0.000000");
                SetVal("PedVarietyMultiplier", "0.000000"); SetVal("VehicleVarietyMultiplier", "0.000000");
                SetVal("Shadow_Distance", "0.000000"); SetVal("Tessellation", "0");
                SetVal("AnisotropicFiltering", "0"); SetVal("MSAA", "0");
                SetVal("FXAA", "true"); SetVal("TXAA", "false");
                SetVal("SSAO", "0"); SetVal("MotionBlurStrength", "0.000000");
                SetVal("DX_Version", "2");
                Log.Success("Ultra FPS preseti uygulandı.");
            }
            else if (presetType == "balanced")
            {
                SetVal("ShadowQuality", "1"); SetVal("ReflectionQuality", "1");
                SetVal("WaterQuality", "1"); SetVal("ParticleQuality", "1");
                SetVal("GrassQuality", "1"); SetVal("ShaderQuality", "1");
                SetVal("PostFX", "1"); SetVal("CityDensity", "0.500000");
                SetVal("Tessellation", "1"); SetVal("AnisotropicFiltering", "4");
                SetVal("MSAA", "0"); SetVal("FXAA", "true"); SetVal("TXAA", "false");
                SetVal("DX_Version", "2");
                Log.Success("Dengeli preset uygulandı.");
            }
            else { Log.Error("Bilinmeyen preset tipi."); return false; }

            doc.Save(settingsPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"GTA V ayarları güncellenirken hata: {ex.Message}");
            return false;
        }
    }

    public void RevertPriorities()
    {
        Log.Info("FiveM işlem öncelikleri varsayılana döndürülüyor...");
        const string baseIfeo = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
        foreach (var proc in FiveMProcesses)
            Utils.DeleteRegistryKey(Registry.LocalMachine, $@"{baseIfeo}\{proc}\PerfOptions");
        Log.Success("FiveM işlem öncelikleri temizlendi.");
    }
}
