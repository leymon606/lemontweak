using System.IO;

namespace MoliTweak.Core;

public sealed class CacheCleaner
{
    public static CacheCleaner Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string TempDir = Path.GetTempPath();

    private static (long bytes, int files) DeleteDirectoryContents(string dirPath, HashSet<string>? excludeNames = null)
    {
        if (!Directory.Exists(dirPath)) return (0, 0);
        long bytesFreed = 0;
        int filesDeleted = 0;

        foreach (var item in Directory.GetFileSystemEntries(dirPath))
        {
            var name = Path.GetFileName(item);
            if (excludeNames != null && excludeNames.Contains(name.ToLowerInvariant())) continue;

            try
            {
                if (File.Exists(item))
                {
                    bytesFreed += new FileInfo(item).Length;
                    File.Delete(item);
                    filesDeleted++;
                }
                else if (Directory.Exists(item))
                {
                    var di = new DirectoryInfo(item);
                    foreach (var f in di.GetFiles("*", SearchOption.AllDirectories))
                    {
                        bytesFreed += f.Length;
                        filesDeleted++;
                    }
                    Directory.Delete(item, recursive: true);
                }
            }
            catch { }
        }
        return (bytesFreed, filesDeleted);
    }

    public (long Bytes, int Files) CleanFiveMCache()
    {
        Log.Info("FiveM Güvenli Önbellek temizliği başlatılıyor...");
        var fivemDir = Utils.GetFiveMDirectory();
        if (fivemDir == null) { Log.Warning("FiveM dizini bulunamadı."); return (0, 0); }

        var dataDir = Path.Combine(fivemDir, "data");
        string[] safeTargets =
        [
            Path.Combine(fivemDir, "crashes"),
            Path.Combine(fivemDir, "logs"),
            Path.Combine(fivemDir, "crashometry"),
            Path.Combine(dataDir, "cache", "subprocess")
        ];

        long totalBytes = 0; int totalFiles = 0;
        foreach (var target in safeTargets)
        {
            var (b, f) = DeleteDirectoryContents(target);
            totalBytes += b; totalFiles += f;
        }

        Log.Success($"FiveM önbellek temizlendi: {totalFiles} dosya ({Utils.FormatBytes(totalBytes)}).");
        return (totalBytes, totalFiles);
    }

    public (long Bytes, int Files) CleanGpuShaderCache()
    {
        Log.Info("GPU Shader Önbelleği temizleniyor...");
        string[] shaderDirs =
        [
            Path.Combine(LocalAppData, "D3DSCache"),
            Path.Combine(LocalAppData, "NVIDIA", "DXCache"),
            Path.Combine(LocalAppData, "NVIDIA", "GLCache"),
            Path.Combine(AppData, "NVIDIA", "ComputeCache"),
            Path.Combine(LocalAppData, "AMD", "DxCache"),
            Path.Combine(LocalAppData, "AMD", "GLCache"),
            Path.Combine(LocalAppData, "Intel", "ShaderCache")
        ];

        long totalBytes = 0; int totalFiles = 0;
        foreach (var dir in shaderDirs)
        {
            var (b, f) = DeleteDirectoryContents(dir);
            totalBytes += b; totalFiles += f;
        }

        Log.Success($"Shader önbelleği temizlendi: {totalFiles} dosya ({Utils.FormatBytes(totalBytes)}).");
        return (totalBytes, totalFiles);
    }

    public (long Bytes, int Files) CleanWindowsTemp()
    {
        Log.Info("Windows Temp dosyaları temizleniyor...");
        long totalBytes = 0; int totalFiles = 0;

        foreach (var t in new[] { TempDir, @"C:\Windows\Temp" })
        {
            var (b, f) = DeleteDirectoryContents(t);
            totalBytes += b; totalFiles += f;
        }

        if (Utils.IsAdmin() && Directory.Exists(@"C:\Windows\Prefetch"))
        {
            var (b, f) = DeleteDirectoryContents(@"C:\Windows\Prefetch");
            totalBytes += b; totalFiles += f;
        }

        Log.Success($"Temp dosyaları temizlendi: {totalFiles} dosya ({Utils.FormatBytes(totalBytes)}).");
        return (totalBytes, totalFiles);
    }

    public (long Bytes, int Files) CleanAll()
    {
        var r1 = CleanFiveMCache();
        var r2 = CleanGpuShaderCache();
        var r3 = CleanWindowsTemp();

        long totalBytes = r1.Bytes + r2.Bytes + r3.Bytes;
        int totalFiles = r1.Files + r2.Files + r3.Files;
        Log.Success($"Toplam: {totalFiles} dosya ({Utils.FormatBytes(totalBytes)}) temizlendi.");
        return (totalBytes, totalFiles);
    }
}
