namespace MoliTweak.Core;

public sealed class ServicesManager
{
    public static ServicesManager Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    private static readonly (string Name, string Description)[] GamingServices =
    [
        ("SysMain", "Superfetch / Bellek Ön Yükleme"),
        ("WSearch", "Windows Arama Dizin Oluşturucu"),
        ("Spooler", "Yazdırma Biriktiricisi"),
        ("CDPSvc", "Connected Devices Platform"),
        ("TrkWks", "Dağıtılmış Bağlantı İzleme"),
        ("wisvc", "Windows Insider Hizmeti"),
        ("DiagTrack", "Telemetry"),
        ("dmwappushservice", "WAP Push")
    ];

    public int ApplyGamingServicesProfile()
    {
        Log.Info("Sistem servisleri korunuyor — hiçbir servis devre dışı bırakılmadı.");
        return 0;
    }

    public void RevertServicesProfile()
    {
        Log.Info("Windows servisleri varsayılana döndürülüyor...");
        (string name, string startType)[] defaults =
        [
            ("SysMain", "auto"), ("WSearch", "auto"), ("Spooler", "auto"),
            ("CDPSvc", "demand"), ("TrkWks", "auto"), ("wisvc", "demand"),
            ("DiagTrack", "auto"), ("dmwappushservice", "demand")
        ];

        int reverted = 0;
        foreach (var (name, startType) in defaults)
        {
            var (success, _, _) = Utils.RunCommand($"sc config {name} start= {startType}");
            if (success)
            {
                reverted++;
                if (startType == "auto") Utils.RunCommand($"sc start {name}");
            }
        }
        Log.Success($"Servisler varsayılana döndürüldü ({reverted} servis güncellendi).");
    }
}
