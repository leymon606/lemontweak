using Microsoft.Win32;

namespace MoliTweak.Core;

public sealed class QosTweaks
{
    public static QosTweaks Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    public void ApplyFiveMQosPriority()
    {
        Log.Info("FiveM QoS (DSCP 46) yapılandırılıyor...");

        Utils.SetRegistryValue(Registry.LocalMachine,
            @"SYSTEM\CurrentControlSet\Services\Tcpip\QoS", "Do not use NLA", RegistryValueKind.String, "1");

        const string qosBase = @"SOFTWARE\Policies\Microsoft\Windows\QoS\FiveM_Priority";
        (string name, string value)[] policies =
        [
            ("Version", "1.0"),
            ("Application Name", "FiveM_GTAProcess.exe"),
            ("Protocol", "*"), ("Local Port", "*"), ("Local IP", "*"),
            ("Local IP Prefix Length", "*"), ("Remote Port", "*"),
            ("Remote IP", "*"), ("Remote IP Prefix Length", "*"),
            ("DSCP Value", "46"), ("Throttle Rate", "-1")
        ];

        foreach (var (name, value) in policies)
            Utils.SetRegistryValue(Registry.LocalMachine, qosBase, name, RegistryValueKind.String, value);

        Utils.RunPowerShell(
            "Get-NetAdapterAdvancedProperty | Where-Object { $_.DisplayName -like '*Large Send Offload*' } | Set-NetAdapterAdvancedProperty -RegistryValue '0' -ErrorAction SilentlyContinue");

        Log.Success("FiveM QoS (DSCP 46) ve LSO ayarları uygulandı.");
    }

    public void RevertQosPriority()
    {
        Log.Info("QoS ayarları kaldırılıyor...");
        Utils.DeleteRegistryKey(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\QoS\FiveM_Priority");
        Log.Success("FiveM QoS politikası kaldırıldı.");
    }
}
