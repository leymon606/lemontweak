using System.IO;

namespace MoliTweak.Core;

public sealed class RestoreManager
{
    public static RestoreManager Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    private readonly string _backupDir;

    private RestoreManager()
    {
        _backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
        Directory.CreateDirectory(_backupDir);
    }

    public bool CreateSystemRestorePoint(string description = "lemonistan_Backup")
    {
        if (!Utils.IsAdmin()) { Log.Warning("Geri yükleme noktası için Yönetici yetkisi gerekli."); return false; }

        Log.Info("Sistem Geri Yükleme Noktası oluşturuluyor...");
        Utils.RunPowerShell("Enable-ComputerRestore -Drive C:");
        var (success, _, err) = Utils.RunPowerShell(
            $"Checkpoint-Computer -Description '{description}' -RestorePointType 'MODIFY_SETTINGS'", 60000);

        if (success || err.Contains("cannot be created"))
        {
            Log.Success(err.Contains("cannot be created")
                ? "Yakın zamanda bir geri yükleme noktası zaten oluşturulmuş."
                : "Sistem Geri Yükleme Noktası başarıyla oluşturuldu.");
            return true;
        }

        Log.Warning($"Geri yükleme noktası oluşturulamadı: {err}");
        return false;
    }

    public bool BackupRegistryKey(string keyPath, string filename)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var outPath = Path.Combine(_backupDir, $"{filename}_{timestamp}.reg");
        var (success, _, _) = Utils.RunCommand($"reg export \"{keyPath}\" \"{outPath}\" /y");
        if (success) Log.Info($"Registry yedeği alındı: {Path.GetFileName(outPath)}");
        return success;
    }

    public void BackupAllCriticalKeys()
    {
        (string path, string name)[] keys =
        [
            (@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "MMCSS_Profile"),
            (@"HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DataCollection_Telemetry"),
            (@"HKCU\System\GameConfigStore", "GameConfigStore"),
            (@"HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR", "GameDVR_Policies"),
            (@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "Tcpip_Parameters"),
            (@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options", "IFEO_Priorities")
        ];
        foreach (var (path, name) in keys)
            BackupRegistryKey(path, name);
    }

    public bool BackupGtaSettings()
    {
        var settingsPath = Utils.GetGtaSettingsFile();
        if (settingsPath == null || !File.Exists(settingsPath)) return false;

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var dest = Path.Combine(_backupDir, $"gta5_settings_backup_{timestamp}.xml");
        try
        {
            File.Copy(settingsPath, dest, overwrite: true);
            Log.Success($"GTA V ayarları yedeklendi: {Path.GetFileName(dest)}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"GTA V ayarları yedeklenemedi: {ex.Message}");
            return false;
        }
    }

    public bool RestoreGtaSettingsFromBackup()
    {
        var settingsPath = Utils.GetGtaSettingsFile();
        if (settingsPath == null) { Log.Warning("GTA V settings.xml bulunamadı."); return false; }

        var backups = Directory.GetFiles(_backupDir, "gta5_settings_backup_*.xml");
        if (backups.Length == 0) { Log.Warning("Kayıtlı bir GTA V yedek bulunamadı."); return false; }

        var latest = backups.OrderByDescending(File.GetLastWriteTime).First();
        try
        {
            File.Copy(latest, settingsPath, overwrite: true);
            Log.Success($"GTA V ayarları yedekten geri yüklendi ({Path.GetFileName(latest)}).");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Geri yükleme hatası: {ex.Message}");
            return false;
        }
    }
}
