using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Win32;
using System;

namespace MoliTweak.Core;

public sealed record TweakStatusItem(string Name, bool Applied);

/// <summary>
/// Registry/dosya durumunu okuyarak hangi tweak'lerin gerçekten uygulanmış olduğunu tespit eder.
/// Dashboard'daki "uygulanan ayar" sayacı ve listesi buradan beslenir — sabit/sahte sayı kullanılmaz.
/// </summary>
public static class TweakStatusChecker
{
    public static List<TweakStatusItem> GetStatus()
    {
        var items = new List<TweakStatusItem>
        {
            new("Fullscreen Optimizations kapalı", RegEquals(Registry.CurrentUser,
                @"System\GameConfigStore", "GameDVR_FSEBehavior", 2)),

            new("Görsel efektler kapalı", RegEquals(Registry.CurrentUser,
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", 0)),

            new("Arka plan uygulamaları kapalı", RegEquals(Registry.CurrentUser,
                @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1)),

            new("Power Throttling kapalı", RegEquals(Registry.LocalMachine,
                @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1)),

            new("Fare ivmesi kapalı", RegEquals(Registry.CurrentUser,
                @"Control Panel\Mouse", "MouseSpeed", "0")),

            new("Bildirimler kapalı", RegEquals(Registry.CurrentUser,
                @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", 0)),

            new("Telemetri kapalı", RegEquals(Registry.LocalMachine,
                @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0)),

            new("GameDVR kapalı", RegEquals(Registry.CurrentUser,
                @"System\GameConfigStore", "GameDVR_Enabled", 0)),

            new("Network Throttling kapalı (MMCSS)", RegEquals(Registry.LocalMachine,
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF))),

            new("Sistem Yanıt Süresi (MMCSS)", RegEquals(Registry.LocalMachine,
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0)),

            new("GPU Donanım Zamanlaması (HAGS)", RegEquals(Registry.LocalMachine,
                @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2)),

            new("GPU TDR Gecikmesi", RegEquals(Registry.LocalMachine,
                @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDelay", 8)),

            new("CPU Core Parking kapalı", RegEquals(Registry.LocalMachine,
                @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60f740d00\0cc5b647-c1df-4637-891a-dec60c185b7a",
                "ValueMax", 100)),

            new("FiveM CPU Önceliği", RegEquals(Registry.LocalMachine,
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FiveM_GTAProcess.exe\PerfOptions",
                "CpuPriorityClass", 3)),

            new("Harici GPU Zorlama",
                Utils.GetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\DirectX\UserGpuPreferences", "GTA5.exe") != null),

            new("FiveM QoS (DSCP 46)", RegEquals(Registry.LocalMachine,
                @"SOFTWARE\Policies\Microsoft\Windows\QoS\FiveM_Priority", "DSCP Value", "46")),

            new("0.5ms Timer Resolution", TimerResolution.Instance.QueryResolution().Current <= 0.6),

            new("Nagle Algoritması kapalı", IsNagleDisabled()),

            new("CitizenFX.ini optimize edildi", IsCitizenFxOptimized()),

            new("Doku Kaybı Düzeltmesi uygulandı", IsTextureFixApplied())
        };

        return items;
    }

    private static bool RegEquals(RegistryKey root, string path, string name, object expected)
    {
        var val = Utils.GetRegistryValue(root, path, name);
        return val != null && string.Equals(val.ToString(), expected.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsServiceDisabled(string serviceName)
    {
        var val = Utils.GetRegistryValue(Registry.LocalMachine,
            $@"SYSTEM\CurrentControlSet\Services\{serviceName}", "Start");
        return val is int i && i == 4;
    }

    private static bool IsNagleDisabled()
    {
        try
        {
            const string interfacesPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";
            using var rootKey = Registry.LocalMachine.OpenSubKey(interfacesPath);
            if (rootKey == null) return false;

            foreach (var sub in rootKey.GetSubKeyNames())
            {
                var val = Utils.GetRegistryValue(Registry.LocalMachine, $@"{interfacesPath}\{sub}", "TCPNoDelay");
                if (val is int i && i == 1) return true;
            }
        }
        catch { }
        return false;
    }

    private static bool IsCitizenFxOptimized()
    {
        try
        {
            var fivemDir = Utils.GetFiveMDirectory();
            if (fivemDir == null) return false;

            var iniPath = Path.Combine(fivemDir, "CitizenFX.ini");
            if (!File.Exists(iniPath))
                iniPath = Path.Combine(Path.GetDirectoryName(fivemDir)!, "CitizenFX.ini");
            if (!File.Exists(iniPath)) return false;

            return File.ReadAllLines(iniPath)
                .Any(l => l.Trim().Equals("DisableNVSPCaps=true", StringComparison.OrdinalIgnoreCase));
        }
        catch { return false; }
    }

    private static bool IsTextureFixApplied()
    {
        try
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(userProfile, "Documents", "Rockstar Games", "GTA V", "commandline.txt");
            if (!File.Exists(path)) return false;
            return File.ReadAllText(path).Contains("-nothreadaffinity");
        }
        catch { return false; }
    }
}
