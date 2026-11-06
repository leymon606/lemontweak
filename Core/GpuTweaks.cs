using System.IO;
using Microsoft.Win32;

namespace MoliTweak.Core;

public sealed class GpuTweaks
{
    public static GpuTweaks Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    private static readonly string[] GpuExes = [..FiveMTweaks.FiveMProcesses, "GTA5.exe"];

    public void ForceDedicatedGpu()
    {
        Log.Info("Harici GPU zorlama ayarları yapılıyor...");
        const string prefKey = @"Software\Microsoft\DirectX\UserGpuPreferences";
        var fivemDir = Utils.GetFiveMDirectory();
        int registered = 0;

        foreach (var exe in GpuExes)
        {
            Utils.SetRegistryValue(Registry.CurrentUser, prefKey, exe, RegistryValueKind.String, "GpuPreference=2;");
            registered++;
            if (fivemDir != null)
            {
                var fullPath = Path.Combine(fivemDir, exe);
                Utils.SetRegistryValue(Registry.CurrentUser, prefKey, fullPath, RegistryValueKind.String, "GpuPreference=2;");
            }
        }

        Log.Success($"Harici GPU zorlama kuralı {registered} işleme tanımlandı.");
    }

    public void ApplyGpuDriverTweaks()
    {
        Log.Info("GPU Sürücü parametreleri optimize ediliyor...");
        const string gfxPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";
        Utils.SetRegistryValue(Registry.LocalMachine, gfxPath, "HwSchMode", RegistryValueKind.DWord, 2);
        Utils.SetRegistryValue(Registry.LocalMachine, gfxPath, "TdrDelay", RegistryValueKind.DWord, 8);
        Utils.SetRegistryValue(Registry.LocalMachine, gfxPath, "TdrDdiDelay", RegistryValueKind.DWord, 8);
        Utils.SetRegistryValue(Registry.LocalMachine, gfxPath, "PlatformSupportMiracast", RegistryValueKind.DWord, 0);

        const string compatPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        var fivemDir = Utils.GetFiveMDirectory();
        if (fivemDir != null)
        {
            foreach (var exe in GpuExes)
            {
                var fp = Path.Combine(fivemDir, exe);
                Utils.SetRegistryValue(Registry.CurrentUser, compatPath, fp, RegistryValueKind.String,
                    "~ HIGHDPIAWARE DISABLEDXMAXIMIZEDWINDOWEDMODE");
            }
        }

        Log.Success("GPU HAGS, TDR ve DWM gecikme düzeltmeleri uygulandı.");
    }

    public void ApplyAll()
    {
        ForceDedicatedGpu();
        ApplyGpuDriverTweaks();
        Log.Success("Tüm GPU optimizasyonları tamamlandı.");
    }
}
