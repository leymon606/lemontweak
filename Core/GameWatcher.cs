using System.Diagnostics;

namespace MoliTweak.Core;

public sealed class GameWatcher
{
    public static GameWatcher Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    private CancellationTokenSource? _cts;
    private bool _gameActive;

    public bool IsRunning => _cts != null && !_cts.IsCancellationRequested;

    public void StartWatcher()
    {
        if (IsRunning) return;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(() => MonitorLoop(token), token);
        Log.Success("Otomatik Game Watcher aktif edildi!");
    }

    public void StopWatcher()
    {
        _cts?.Cancel();
        _cts = null;
        _gameActive = false;
        TimerResolution.Instance.ResetResolution();
        Log.Info("Game Watcher durduruldu.");
    }

    private void MonitorLoop(CancellationToken token)
    {
        Log.Info("Game Watcher döngüsü dinlemede...");
        while (!token.IsCancellationRequested)
        {
            Process? fivemProc = null;
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    var name = p.ProcessName.ToLowerInvariant();
                    if (name.Contains("fivem_gtaprocess") || name.Contains("fivem_b"))
                    {
                        fivemProc = p;
                        break;
                    }
                }
                catch { }
            }

            if (fivemProc != null && !_gameActive)
            {
                _gameActive = true;
                Log.Success($"FiveM Algılandı! (PID: {fivemProc.Id}) -> Otomatik Boost devreye giriyor...");
                TimerResolution.Instance.SetResolution(0.5);
                try
                {
                    fivemProc.PriorityClass = ProcessPriorityClass.High;
                    Log.Success("FiveM işlem önceliği [HIGH] olarak kilitlendi.");
                }
                catch { }
                RamCleaner.Instance.CleanRam();
            }
            else if (fivemProc == null && _gameActive)
            {
                _gameActive = false;
                Log.Info("FiveM kapatıldı. Sistem normal moda döndürülüyor.");
                TimerResolution.Instance.ResetResolution();
            }

            if (_gameActive)
            {
                var (percent, _, _) = RamCleaner.Instance.GetMemoryInfo();
                if (percent > 85.0)
                {
                    Log.Warning($"RAM kullanımı yüksek (%{percent:F1}). Otomatik temizlik...");
                    RamCleaner.Instance.CleanRam();
                }
            }

            Thread.Sleep(3000);
        }
    }
}
