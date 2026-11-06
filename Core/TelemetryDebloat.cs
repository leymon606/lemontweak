using Microsoft.Win32;

namespace MoliTweak.Core;

public sealed class TelemetryDebloat
{
    public static TelemetryDebloat Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    public void DisableTelemetryRegistry()
    {
        Log.Info("Windows Telemetri ve Veri Toplama ayarları yapılıyor...");
        (RegistryKey root, string path, string name, RegistryValueKind kind, object value)[] tweaks =
        [
            (Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", RegistryValueKind.DWord, 0),
            (Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "MaxTelemetryAllowed", RegistryValueKind.DWord, 0),
            (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", RegistryValueKind.DWord, 0),
            (Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", RegistryValueKind.DWord, 0),
            (Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", RegistryValueKind.DWord, 0),
            (Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", RegistryValueKind.DWord, 0),
            (Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", RegistryValueKind.DWord, 0),
            (Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", RegistryValueKind.DWord, 0),
            (Registry.CurrentUser, @"Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", RegistryValueKind.DWord, 0),
            (Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled", RegistryValueKind.DWord, 1)
        ];

        int success = 0;
        foreach (var (root, path, name, kind, value) in tweaks)
            if (Utils.SetRegistryValue(root, path, name, kind, value)) success++;

        Log.Success($"Telemetri ayarları uygulandı ({success}/{tweaks.Length}).");
    }

    public void DisableTelemetryServices()
    {
        // Hiçbir servis kapatılmıyor — servis koruması aktif.
    }

    public void DisableGameDvr()
    {
        Log.Info("GameDVR ve Xbox arka plan yakalama kapatılıyor...");
        (RegistryKey root, string path, string name, RegistryValueKind kind, object value)[] tweaks =
        [
            (Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", RegistryValueKind.DWord, 0),
            (Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_FSEBehaviorMode", RegistryValueKind.DWord, 2),
            (Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", RegistryValueKind.DWord, 1),
            (Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", RegistryValueKind.DWord, 1),
            (Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_EFSEFeatureFlags", RegistryValueKind.DWord, 0),
            (Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", RegistryValueKind.DWord, 0),
            (Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", RegistryValueKind.DWord, 0),
            (Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "HistoricalCaptureEnabled", RegistryValueKind.DWord, 0),
            (Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AudioCaptureEnabled", RegistryValueKind.DWord, 0),
            (Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", RegistryValueKind.DWord, 1)
        ];
        foreach (var (root, path, name, kind, value) in tweaks)
            Utils.SetRegistryValue(root, path, name, kind, value);
        Log.Success("GameDVR ve arka plan yakalama kapatıldı.");
    }

    public void ApplyAll()
    {
        DisableTelemetryRegistry();
        DisableGameDvr();
        Log.Success("Tüm Telemetri ve GameDVR optimizasyonları uygulandı.");
    }

    public void RevertToDefault()
    {
        Log.Info("Telemetri ve GameDVR ayarları varsayılana döndürülüyor...");
        Utils.SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", RegistryValueKind.DWord, 1);
        Utils.RunCommand("sc config DiagTrack start= auto");
        Utils.RunCommand("sc start DiagTrack");
        Utils.RunCommand("sc config WerSvc start= demand");
        Utils.SetRegistryValue(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", RegistryValueKind.DWord, 1);
        Utils.SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", RegistryValueKind.DWord, 1);
        Utils.SetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", RegistryValueKind.DWord, 1);
        Log.Success("Telemetri ve GameDVR varsayılana döndürüldü.");
    }
}
