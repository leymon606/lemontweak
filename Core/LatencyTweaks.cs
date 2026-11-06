using Microsoft.Win32;

namespace MoliTweak.Core;

public sealed class LatencyTweaks
{
    public static LatencyTweaks Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    public bool ApplyBcdeditTweaks()
    {
        Log.Info("BCDEDIT DPC ve Zamanlayıcı gecikme ayarları uygulanıyor...");
        string[] commands =
        [
            "bcdedit /set disabledynamictick yes",
            "bcdedit /set useplatformclock no",
            "bcdedit /set useplatformtick yes",
            "bcdedit /set syntheticcomputertimer yes",
            "bcdedit /set tscsyncpolicy Enhanced"
        ];
        int success = 0;
        foreach (var cmd in commands)
            if (Utils.RunCommand(cmd).Success) success++;

        Log.Success($"BCDEDIT gecikme ayarları uygulandı ({success}/{commands.Length}).");
        return success > 0;
    }

    public bool ApplyMmcssTweaks()
    {
        Log.Info("MMCSS Oyun Önceliklendirme ayarları yapılıyor...");
        const string basePath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
        const string gamesPath = basePath + @"\Tasks\Games";

        int success = 0;
        (string path, string name, RegistryValueKind kind, object value)[] tweaks =
        [
            (basePath, "NetworkThrottlingIndex", RegistryValueKind.DWord, unchecked((int)0xFFFFFFFF)),
            (basePath, "SystemResponsiveness", RegistryValueKind.DWord, 0),
            (basePath, "NoLazyMode", RegistryValueKind.DWord, 1),
            (basePath, "AlwaysOn", RegistryValueKind.DWord, 1),
            (gamesPath, "GPU Priority", RegistryValueKind.DWord, 8),
            (gamesPath, "Priority", RegistryValueKind.DWord, 6),
            (gamesPath, "Scheduling Category", RegistryValueKind.String, "High"),
            (gamesPath, "SFIO Priority", RegistryValueKind.String, "High"),
            (gamesPath, "Background Only", RegistryValueKind.String, "False"),
            (gamesPath, "Clock Rate", RegistryValueKind.DWord, 10000)
        ];

        foreach (var (path, name, kind, value) in tweaks)
            if (Utils.SetRegistryValue(Registry.LocalMachine, path, name, kind, value)) success++;

        Log.Success($"MMCSS Oyun öncelikleri ayarlandı ({success}/{tweaks.Length}).");
        return success > 0;
    }

    public bool ApplyGpuScheduling()
    {
        Log.Info("HAGS optimize ediliyor...");
        return Utils.SetRegistryValue(Registry.LocalMachine,
            @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", RegistryValueKind.DWord, 2);
    }

    public void ApplyAll()
    {
        ApplyBcdeditTweaks();
        ApplyMmcssTweaks();
        ApplyGpuScheduling();
        Log.Success("Tüm gecikme giderme ayarları uygulandı.");
    }

    public void RevertToDefault()
    {
        Log.Info("BCDEDIT ve MMCSS ayarları varsayılana döndürülüyor...");
        string[] revertCmds =
        [
            "bcdedit /deletevalue disabledynamictick",
            "bcdedit /deletevalue useplatformclock",
            "bcdedit /deletevalue useplatformtick",
            "bcdedit /deletevalue syntheticcomputertimer",
            "bcdedit /deletevalue tscsyncpolicy"
        ];
        foreach (var cmd in revertCmds) Utils.RunCommand(cmd);

        const string basePath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
        const string gamesPath = basePath + @"\Tasks\Games";
        Utils.SetRegistryValue(Registry.LocalMachine, basePath, "NetworkThrottlingIndex", RegistryValueKind.DWord, 10);
        Utils.SetRegistryValue(Registry.LocalMachine, basePath, "SystemResponsiveness", RegistryValueKind.DWord, 20);
        Utils.SetRegistryValue(Registry.LocalMachine, gamesPath, "GPU Priority", RegistryValueKind.DWord, 8);
        Utils.SetRegistryValue(Registry.LocalMachine, gamesPath, "Priority", RegistryValueKind.DWord, 2);
        Utils.SetRegistryValue(Registry.LocalMachine, gamesPath, "Scheduling Category", RegistryValueKind.String, "Medium");
        Log.Success("Gecikme ayarları varsayılana döndürüldü.");
    }
}
