using System.Runtime.InteropServices;

namespace MoliTweak.Core;

public sealed class TimerResolution
{
    public static TimerResolution Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    [DllImport("ntdll.dll")]
    private static extern int NtSetTimerResolution(uint DesiredResolution, bool SetResolution, out uint CurrentResolution);

    [DllImport("ntdll.dll")]
    private static extern int NtQueryTimerResolution(out uint MinimumResolution, out uint MaximumResolution, out uint CurrentResolution);

    public (double Min, double Max, double Current) QueryResolution()
    {
        var status = NtQueryTimerResolution(out uint min, out uint max, out uint cur);
        if (status == 0)
            return (min / 10000.0, max / 10000.0, cur / 10000.0);
        return (15.625, 0.5, 15.625);
    }

    public double SetResolution(double desiredMs = 0.5)
    {
        uint desired100ns = (uint)(desiredMs * 10000);
        NtSetTimerResolution(desired100ns, true, out uint currentRes);

        var (_, _, cur) = QueryResolution();
        if (cur <= desiredMs + 0.1)
        {
            Log.Success($"0.5ms Timer Resolution Aktif! Güncel: {cur:F4} ms.");
        }
        else
        {
            Log.Warning($"Timer çözünürlüğü ayarlanamadı. Güncel: {cur:F4} ms.");
        }
        return cur;
    }

    public double ResetResolution()
    {
        NtSetTimerResolution(156250, false, out _);
        var (_, _, cur) = QueryResolution();
        Log.Info($"Zamanlayıcı varsayılana döndürüldü: {cur:F4} ms.");
        return cur;
    }
}
