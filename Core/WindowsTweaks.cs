using Microsoft.Win32;

namespace MoliTweak.Core;

public sealed class WindowsTweaks
{
    public static WindowsTweaks Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    public void DisableFullscreenOptimizations()
    {
        Log.Info("Fullscreen Optimizations kapatılıyor...");

        Utils.SetRegistryValue(Registry.CurrentUser,
            @"System\GameConfigStore", "GameDVR_FSEBehavior", RegistryValueKind.DWord, 2);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"System\GameConfigStore", "GameDVR_FSEBehaviorMode", RegistryValueKind.DWord, 2);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", RegistryValueKind.DWord, 1);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", RegistryValueKind.DWord, 1);

        Log.Success("Fullscreen Optimizations kapatıldı — oyunlar gerçek tam ekran modunda çalışacak.");
    }

    public void DisableVisualEffects()
    {
        Log.Info("Windows görsel efektleri kapatılıyor...");

        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", RegistryValueKind.DWord, 0);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewAlphaSelect", RegistryValueKind.DWord, 0);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewShadow", RegistryValueKind.DWord, 0);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Desktop", "DragFullWindows", RegistryValueKind.String, "0");
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Desktop", "MenuShowDelay", RegistryValueKind.String, "0");
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Desktop\WindowMetrics", "MinAnimate", RegistryValueKind.String, "0");
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\DWM", "EnableAeroPeek", RegistryValueKind.DWord, 0);

        Log.Success("Animasyon, şeffaflık ve gölge efektleri kapatıldı.");
    }

    public void DisableBackgroundApps()
    {
        Log.Info("Arka plan uygulamaları kapatılıyor...");

        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", RegistryValueKind.DWord, 1);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", RegistryValueKind.DWord, 0);

        Log.Success("UWP/Store arka plan uygulamaları devre dışı bırakıldı.");
    }

    public void DisablePowerThrottling()
    {
        Log.Info("Power Throttling kapatılıyor...");

        Utils.SetRegistryValue(Registry.LocalMachine,
            @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", RegistryValueKind.DWord, 1);

        Log.Success("CPU güç kısıtlaması kapatıldı — işlemci tam performansta çalışacak.");
    }

    public void DisableMouseAcceleration()
    {
        Log.Info("Fare ivmesi (acceleration) kapatılıyor...");

        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Mouse", "MouseSpeed", RegistryValueKind.String, "0");
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Mouse", "MouseThreshold1", RegistryValueKind.String, "0");
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Mouse", "MouseThreshold2", RegistryValueKind.String, "0");

        Log.Success("Fare ivmesi kapatıldı — 1:1 ham fare hareketi aktif.");
    }

    public void DisableNotifications()
    {
        Log.Info("Bildirimler kapatılıyor...");

        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", RegistryValueKind.DWord, 0);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", RegistryValueKind.DWord, 1);

        Log.Success("Toast bildirimleri ve bildirim merkezi kapatıldı.");
    }

    public void EnableGameModeAndDisableGameDVR()
    {
        Log.Info("Oyun Modu aktif ediliyor ve Xbox Game DVR kapatılıyor...");

        // Oyun modunu aç
        Utils.SetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", RegistryValueKind.DWord, 1);
        Utils.SetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", RegistryValueKind.DWord, 1);

        // Game DVR (arka plan kaydı) kapat
        Utils.SetRegistryValue(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", RegistryValueKind.DWord, 0);
        Utils.SetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", RegistryValueKind.DWord, 0);

        Log.Success("Oyun Modu açıldı ve Game DVR arka plan kaydı kapatıldı (FPS Artışı).");
    }

    public void ApplyAll()
    {
        DisableFullscreenOptimizations();
        DisableVisualEffects();
        DisableBackgroundApps();
        DisablePowerThrottling();
        DisableMouseAcceleration();
        DisableNotifications();
        EnableGameModeAndDisableGameDVR();
    }

    public void RevertToDefault()
    {
        Log.Info("Windows tweak ayarları varsayılana döndürülüyor...");

        Utils.DeleteRegistryValue(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_FSEBehavior");
        Utils.DeleteRegistryValue(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_FSEBehaviorMode");
        Utils.DeleteRegistryValue(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode");
        Utils.DeleteRegistryValue(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible");

        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", RegistryValueKind.DWord, 1);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewAlphaSelect", RegistryValueKind.DWord, 1);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewShadow", RegistryValueKind.DWord, 1);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Desktop", "DragFullWindows", RegistryValueKind.String, "1");
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Desktop", "MenuShowDelay", RegistryValueKind.String, "400");
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Desktop\WindowMetrics", "MinAnimate", RegistryValueKind.String, "1");
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\DWM", "EnableAeroPeek", RegistryValueKind.DWord, 1);

        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", RegistryValueKind.DWord, 0);
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", RegistryValueKind.DWord, 1);

        Utils.DeleteRegistryValue(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff");

        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Mouse", "MouseSpeed", RegistryValueKind.String, "1");
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Mouse", "MouseThreshold1", RegistryValueKind.String, "6");
        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Control Panel\Mouse", "MouseThreshold2", RegistryValueKind.String, "10");

        Utils.SetRegistryValue(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", RegistryValueKind.DWord, 1);
        Utils.DeleteRegistryValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter");

        Utils.DeleteRegistryValue(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode");
        Utils.DeleteRegistryValue(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled");
        Utils.DeleteRegistryValue(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled");
        Utils.DeleteRegistryValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled");

        Log.Success("Windows tweak ayarları varsayılana döndürüldü.");
    }
}
