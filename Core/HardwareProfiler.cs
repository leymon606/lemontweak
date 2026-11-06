using System;
using System.Collections.Generic;
using System.Management;
using Microsoft.Win32;

namespace MoliTweak.Core;

public record HardwareSpecs
{
    public string GpuName { get; init; } = "Bilinmeyen Ekran Kartı";
    public int VramMb { get; init; } = 4096;
    public double TotalRamGb { get; init; }
    public int CpuCores { get; init; }
    public int CpuPhysical { get; init; }
    public string Tier { get; init; } = "mid";
    public int Score { get; init; }
    public string Reason { get; init; } = "";
}

public sealed class HardwareProfiler
{
    public static HardwareProfiler Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    private HardwareSpecs? _cached;

    public HardwareSpecs ScanHardware()
    {
        if (_cached != null) return _cached;

        var totalRamGb = Math.Round(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024.0 * 1024 * 1024), 1);
        int cpuLogical = Environment.ProcessorCount;
        int cpuPhysical = cpuLogical / 2;

        string gpuName = "Bilinmeyen Ekran Kartı";
        int vramMb = 4096;

        try
        {
            var baseKey = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
            using var root = Registry.LocalMachine.OpenSubKey(baseKey);
            if (root != null)
            {
                foreach (var sub in root.GetSubKeyNames())
                {
                    if (!int.TryParse(sub, out _)) continue;
                    using var sk = root.OpenSubKey(sub);
                    if (sk == null) continue;

                    var desc = sk.GetValue("DriverDesc") as string;
                    if (string.IsNullOrEmpty(desc)) continue;
                    var lower = desc.ToLowerInvariant();
                    if (lower.Contains("virtual") || lower.Contains("remote") || lower.Contains("parsec") ||
                        lower.Contains("anydesk") || lower.Contains("basic")) continue;

                    gpuName = desc;
                    var memSize = sk.GetValue("HardwareInformation.qwMemorySize");
                    if (memSize is long ms && ms > 0)
                        vramMb = (int)(ms / (1024 * 1024));
                    else if (memSize is int mi && mi > 0)
                        vramMb = mi / (1024 * 1024);
                    break;
                }
            }
        }
        catch { }

        var (tier, score, reason) = ClassifyTier(gpuName, totalRamGb, cpuLogical, cpuPhysical, vramMb);

        _cached = new HardwareSpecs
        {
            GpuName = gpuName, VramMb = vramMb, TotalRamGb = totalRamGb,
            CpuCores = cpuLogical, CpuPhysical = cpuPhysical,
            Tier = tier, Score = score, Reason = reason
        };
        return _cached;
    }

    private static (string tier, int score, string reason) ClassifyTier(
        string gpuName, double ramGb, int cpuLogical, int cpuPhysical, int vramMb)
    {
        var gpu = gpuName.ToLowerInvariant();

        string[] highFlags = ["rtx 3070", "rtx 3080", "rtx 3090", "rtx 4070", "rtx 4080", "rtx 4090",
            "rx 6800", "rx 6900", "rx 7800", "rx 7900", "rx 7950"];

        string[] lowFlags = ["intel", "uhd", "iris", "vega", "gtx 750", "gtx 950", "gtx 960", "gtx 1050",
            "gt 1030", "gt 730", "gt 710", "mx", "geforce 9", "radeon r7", "radeon r5",
            "radeon r9 2", "radeon hd", "rx 550", "rx 460", "rx 560"];

        if ((Array.Exists(highFlags, h => gpu.Contains(h)) && ramGb >= 15.5) ||
            (ramGb >= 30.0 && cpuLogical >= 12 && gpu.Contains("rtx")))
            return ("high", 95, "Üst Seviye (High-End): Yüksek VRAM ve Güçlü Ekran Kartı");

        if (Array.Exists(lowFlags, l => gpu.Contains(l)) || ramGb <= 8.5 || cpuLogical <= 4)
            return ("low", 40, "Giriş Seviyesi (Low-End): Sınırlı Donanım / Eski GPU");

        return ("mid", 75, "Orta Seviye (Mid-End): Dengeli Donanım (GTX 1660 / RTX 2060 / RTX 3060 / RX 6600)");
    }

    public void ApplyProfile(string tier)
    {
        var specs = ScanHardware();
        Log.Info($"[{tier.ToUpper()}-END] Donanım Profili uygulanıyor... (GPU: {specs.GpuName}, RAM: {specs.TotalRamGb}GB)");

        RestoreManager.Instance.BackupAllCriticalKeys();

        switch (tier.ToLower())
        {
            case "low":
                FiveMTweaks.Instance.ApplyGtaPreset("ultra_fps");
                TextureLossFix.Instance.ApplyTextureLossFix(2048);
                TelemetryDebloat.Instance.ApplyAll();
                LatencyTweaks.Instance.ApplyAll();
                TimerResolution.Instance.SetResolution(0.5);
                NetworkTweaks.Instance.DisableNagleAlgorithm();
                WindowsTweaks.Instance.ApplyAll();
                GpuTweaks.Instance.ForceDedicatedGpu();
                RamCleaner.Instance.CleanRam();
                Log.Success("[LOW-END] Maksimum FPS Kurtarma Profili uygulandı!");
                break;

            case "mid":
                FiveMTweaks.Instance.ApplyGtaPreset("balanced");
                TextureLossFix.Instance.ApplyTextureLossFix(4096);
                TelemetryDebloat.Instance.ApplyAll();
                LatencyTweaks.Instance.ApplyAll();
                TimerResolution.Instance.SetResolution(0.5);
                NetworkTweaks.Instance.DisableNagleAlgorithm();
                WindowsTweaks.Instance.ApplyAll();
                GpuTweaks.Instance.ApplyAll();
                MsiMode.Instance.EnableMsiModeForGamingDevices();
                PowerTweaks.Instance.ApplyAll();
                RamCleaner.Instance.CleanRam();
                Log.Success("[MID-END] Dengeli 100+ FPS Profili uygulandı!");
                break;

            case "high":
                FiveMTweaks.Instance.ApplyGtaPreset("balanced");
                TextureLossFix.Instance.ApplyTextureLossFix(6144);
                GpuTweaks.Instance.ApplyAll();
                MsiMode.Instance.EnableMsiModeForGamingDevices();
                TelemetryDebloat.Instance.ApplyAll();
                LatencyTweaks.Instance.ApplyAll();
                TimerResolution.Instance.SetResolution(0.5);
                NetworkTweaks.Instance.DisableNagleAlgorithm();
                WindowsTweaks.Instance.ApplyAll();
                PowerTweaks.Instance.ApplyAll();
                RamCleaner.Instance.CleanRam();
                Log.Success("[HIGH-END] Canavar / Rekabetçi Profili uygulandı!");
                break;

            default:
                Log.Error($"Bilinmeyen profil: {tier}");
                break;
        }
    }
}
