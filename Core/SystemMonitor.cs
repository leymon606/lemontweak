using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace MoliTweak.Core;

public sealed class SystemMonitor
{
    public static SystemMonitor Instance { get; } = new();

    public double CpuPercent { get; private set; }
    public double RamPercent { get; private set; }
    public long RamUsedBytes { get; private set; }
    public long RamTotalBytes { get; private set; }
    public double DiskPercent { get; private set; }
    public long DiskUsedGb { get; private set; }
    public long DiskTotalGb { get; private set; }
    public double NetworkKBps { get; private set; }

    private long _prevIdle, _prevKernel, _prevUser;
    private long _prevNetBytes;
    private DateTime _prevNetTime = DateTime.MinValue;

    [DllImport("kernel32.dll")]
    private static extern bool GetSystemTimes(out long idleTime, out long kernelTime, out long userTime);

    public void Refresh()
    {
        RefreshCpu();
        RefreshRam();
        RefreshDisk();
        RefreshNetwork();
    }

    private void RefreshCpu()
    {
        try
        {
            if (GetSystemTimes(out long idle, out long kernel, out long user))
            {
                if (_prevKernel != 0 || _prevUser != 0)
                {
                    long idleDelta = idle - _prevIdle;
                    long totalDelta = (kernel - _prevKernel) + (user - _prevUser);

                    if (totalDelta > 0)
                        CpuPercent = Math.Round((1.0 - (double)idleDelta / totalDelta) * 100.0, 1);
                }
                _prevIdle = idle;
                _prevKernel = kernel;
                _prevUser = user;
            }
        }
        catch { }
    }

    private void RefreshRam()
    {
        try
        {
            var (pct, used, total) = RamCleaner.Instance.GetMemoryInfo();
            RamPercent = pct;
            RamUsedBytes = (long)used;
            RamTotalBytes = (long)total;
        }
        catch { }
    }

    private void RefreshDisk()
    {
        try
        {
            var drive = new DriveInfo("C");
            var used = drive.TotalSize - drive.AvailableFreeSpace;
            DiskPercent = Math.Round(100.0 * used / drive.TotalSize, 1);
            DiskUsedGb = used / (1024L * 1024 * 1024);
            DiskTotalGb = drive.TotalSize / (1024L * 1024 * 1024);
        }
        catch { }
    }

    private void RefreshNetwork()
    {
        try
        {
            long total = 0;
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var stats = ni.GetIPStatistics();
                    total += stats.BytesReceived + stats.BytesSent;
                }
            }

            var now = DateTime.UtcNow;
            if (_prevNetTime != DateTime.MinValue)
            {
                var elapsed = (now - _prevNetTime).TotalSeconds;
                if (elapsed > 0.5)
                    NetworkKBps = Math.Round(Math.Max(0, (total - _prevNetBytes) / elapsed / 1024.0), 1);
            }
            _prevNetBytes = total;
            _prevNetTime = now;
        }
        catch { NetworkKBps = 0; }
    }
}
