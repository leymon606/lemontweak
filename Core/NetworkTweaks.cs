using Microsoft.Win32;

namespace MoliTweak.Core;

public sealed class NetworkTweaks
{
    public static NetworkTweaks Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    public bool DisableNagleAlgorithm()
    {
        Log.Info("Nagle Algoritması & TCPNoDelay optimize ediliyor...");
        const string interfacesPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";
        int success = 0;

        try
        {
            using var rootKey = Registry.LocalMachine.OpenSubKey(interfacesPath);
            if (rootKey == null) return false;

            foreach (var sub in rootKey.GetSubKeyNames())
            {
                var subPath = $@"{interfacesPath}\{sub}";
                var r1 = Utils.SetRegistryValue(Registry.LocalMachine, subPath, "TcpAckFrequency", RegistryValueKind.DWord, 1);
                var r2 = Utils.SetRegistryValue(Registry.LocalMachine, subPath, "TCPNoDelay", RegistryValueKind.DWord, 1);
                Utils.SetRegistryValue(Registry.LocalMachine, subPath, "TcpDelAckTicks", RegistryValueKind.DWord, 0);
                if (r1 && r2) success++;
            }

            Log.Success($"TCP paket gecikmeleri kaldırıldı ({success} ağ bağdaştırıcısı).");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Nagle algoritması ayarlanırken hata: {ex.Message}");
            return false;
        }
    }

    public void RevertNetwork()
    {
        Log.Info("Nagle ayarları varsayılana döndürülüyor...");
        const string interfacesPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";
        try
        {
            using var rootKey = Registry.LocalMachine.OpenSubKey(interfacesPath);
            if (rootKey != null)
            {
                foreach (var sub in rootKey.GetSubKeyNames())
                {
                    var subPath = $@"{interfacesPath}\{sub}";
                    Utils.DeleteRegistryValue(Registry.LocalMachine, subPath, "TcpAckFrequency");
                    Utils.DeleteRegistryValue(Registry.LocalMachine, subPath, "TCPNoDelay");
                    Utils.DeleteRegistryValue(Registry.LocalMachine, subPath, "TcpDelAckTicks");
                }
            }
        }
        catch { }
        Log.Success("Nagle ayarları varsayılana döndürüldü.");
    }
}
