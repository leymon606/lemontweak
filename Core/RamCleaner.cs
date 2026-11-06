using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MoliTweak.Core;

public sealed class RamCleaner
{
    public static RamCleaner Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("psapi.dll")]
    private static extern bool EmptyWorkingSet(IntPtr hProcess);

    public (long FreedBytes, int ProcessesCleaned) CleanRam()
    {
        Log.Info("RAM ve Standby List temizliği başlatılıyor...");

        var gcInfo = GC.GetGCMemoryInfo();
        long usedBefore = gcInfo.TotalAvailableMemoryBytes - gcInfo.TotalAvailableMemoryBytes; // placeholder

        // GCMemoryInfo doesn't give "used" easily, use Performance Counter approach
        var memBefore = GetUsedPhysicalMemory();

        const uint PROCESS_SET_QUOTA = 0x0100;
        const uint PROCESS_QUERY_INFORMATION = 0x0400;
        int cleaned = 0;

        foreach (var proc in Process.GetProcesses())
        {
            try
            {
                if (proc.Id <= 4) continue;
                var handle = OpenProcess(PROCESS_SET_QUOTA | PROCESS_QUERY_INFORMATION, false, proc.Id);
                if (handle != IntPtr.Zero)
                {
                    EmptyWorkingSet(handle);
                    CloseHandle(handle);
                    cleaned++;
                }
            }
            catch { }
        }

        var memAfter = GetUsedPhysicalMemory();
        long freed = Math.Max(0, memBefore - memAfter);

        Log.Success($"RAM Temizlendi! {cleaned} işlem optimize edildi. ({Utils.FormatBytes(freed)} alan kazanıldı).");
        return (freed, cleaned);
    }

    private static long GetUsedPhysicalMemory()
    {
        try
        {
            var gcInfo = GC.GetGCMemoryInfo();
            long total = gcInfo.TotalAvailableMemoryBytes;
            // Use native GlobalMemoryStatusEx for accurate reading
            var status = new MEMORYSTATUSEX();
            status.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            GlobalMemoryStatusEx(ref status);
            return (long)(status.ullTotalPhys - status.ullAvailPhys);
        }
        catch { return 0; }
    }

    [DllImport("kernel32.dll")]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    public (double Percent, long Used, long Total) GetMemoryInfo()
    {
        var status = new MEMORYSTATUSEX();
        status.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        GlobalMemoryStatusEx(ref status);
        return (status.dwMemoryLoad, (long)(status.ullTotalPhys - status.ullAvailPhys), (long)status.ullTotalPhys);
    }
}
