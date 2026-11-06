using Microsoft.Win32;

namespace MoliTweak.Core;

public sealed class MsiMode
{
    public static MsiMode Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    public int EnableMsiModeForGamingDevices()
    {
        Log.Info("MSI (Message Signaled Interrupts) Modu taranıyor...");
        const string pciRoot = @"SYSTEM\CurrentControlSet\Enum\PCI";
        int enabled = 0;

        try
        {
            using var rootKey = Registry.LocalMachine.OpenSubKey(pciRoot);
            if (rootKey == null) return 0;

            foreach (var dev in rootKey.GetSubKeyNames())
            {
                var devPath = $@"{pciRoot}\{dev}";
                try
                {
                    using var subKey = Registry.LocalMachine.OpenSubKey(devPath);
                    if (subKey == null) continue;

                    foreach (var inst in subKey.GetSubKeyNames())
                    {
                        var instPath = $@"{devPath}\{inst}";
                        string devDesc = "", devClass = "";
                        try
                        {
                            using var instKey = Registry.LocalMachine.OpenSubKey(instPath);
                            devDesc = instKey?.GetValue("DeviceDesc") as string ?? "";
                            devClass = instKey?.GetValue("Class") as string ?? "";
                        }
                        catch { }

                        var classLower = devClass.ToLowerInvariant();
                        var descLower = devDesc.ToLowerInvariant();

                        if (classLower is "display" or "net" ||
                            descLower.Contains("nvidia") || descLower.Contains("radeon") || descLower.Contains("geforce"))
                        {
                            var msiPath = $@"{instPath}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties";
                            if (Utils.SetRegistryValue(Registry.LocalMachine, msiPath, "MSISupported", RegistryValueKind.DWord, 1))
                            {
                                Utils.SetRegistryValue(Registry.LocalMachine, msiPath, "MessageNumberLimit", RegistryValueKind.DWord, 16);
                                enabled++;
                            }
                        }
                    }
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"MSI modu taranırken hata: {ex.Message}");
        }

        Log.Success($"MSI Modu {enabled} aygıta başarıyla tanımlandı.");
        return enabled;
    }
}
