using System.Diagnostics;
using System.IO;

namespace MoliTweak.Core;

public record BenchmarkResult
{
    public double CpuMops { get; init; }
    public double RamMBps { get; init; }
    public double BgCpu { get; init; }
    public double BgRam { get; init; }
    public int Score { get; init; }
    public DateTime Timestamp { get; init; }
}

public sealed class BenchmarkRunner
{
    public static BenchmarkRunner Instance { get; } = new();
    private static readonly Logger Log = Logger.Instance;

    private static string ResultDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lemonistan");

    private static string ResultFile => Path.Combine(ResultDir, "benchmark.txt");

    public BenchmarkResult Run()
    {
        Log.Info("Benchmark başlatılıyor (yaklaşık 5 saniye)...");

        SystemMonitor.Instance.Refresh();
        double bgCpu = SystemMonitor.Instance.CpuPercent;
        double bgRam = SystemMonitor.Instance.RamPercent;

        double cpuMops = RunCpuBenchmark();
        double ramMBps = RunRamBenchmark();

        int cpuPts = (int)Math.Min(40, cpuMops / 1.5);
        int ramPts = (int)Math.Min(30, ramMBps / 400.0 * 30);
        int bgCpuPts = (int)Math.Max(0, 15 - bgCpu * 0.15);
        int bgRamPts = (int)Math.Max(0, 15 - Math.Max(0, bgRam - 40) * 0.3);
        int score = Math.Clamp(cpuPts + ramPts + bgCpuPts + bgRamPts, 0, 100);

        var result = new BenchmarkResult
        {
            CpuMops = Math.Round(cpuMops, 1),
            RamMBps = Math.Round(ramMBps, 0),
            BgCpu = Math.Round(bgCpu, 1),
            BgRam = Math.Round(bgRam, 1),
            Score = score,
            Timestamp = DateTime.Now
        };

        SaveResult(result);
        Log.Success($"Benchmark tamamlandı — Skor: {score}/100 | CPU: {result.CpuMops}M ops/s | RAM: {result.RamMBps} MB/s");
        return result;
    }

    private static double RunCpuBenchmark()
    {
        long totalOps = 0;
        int threads = Environment.ProcessorCount;
        var sw = Stopwatch.StartNew();
        var duration = TimeSpan.FromSeconds(3);

        Parallel.For(0, threads, _ =>
        {
            long local = 0;
            while (sw.Elapsed < duration)
            {
                for (int i = 2; i < 5000; i++)
                {
                    bool prime = true;
                    for (int j = 2; j * j <= i; j++)
                    {
                        if (i % j == 0) { prime = false; break; }
                    }
                    if (prime) local++;
                }
            }
            Interlocked.Add(ref totalOps, local);
        });

        return totalOps / 1_000_000.0;
    }

    private static double RunRamBenchmark()
    {
        const int size = 64 * 1024 * 1024;
        var sw = Stopwatch.StartNew();

        var arr = new byte[size];
        for (int i = 0; i < size; i += 64)
            arr[i] = (byte)(i & 0xFF);
        long sum = 0;
        for (int i = 0; i < size; i += 64)
            sum += arr[i];
        sw.Stop();

        GC.KeepAlive(sum);
        return (size / (1024.0 * 1024)) / sw.Elapsed.TotalSeconds;
    }

    public BenchmarkResult? LoadLastResult()
    {
        try
        {
            if (!File.Exists(ResultFile)) return null;
            var lines = File.ReadAllLines(ResultFile);
            if (lines.Length < 6) return null;

            return new BenchmarkResult
            {
                CpuMops = double.Parse(lines[0]),
                RamMBps = double.Parse(lines[1]),
                BgCpu = double.Parse(lines[2]),
                BgRam = double.Parse(lines[3]),
                Score = int.Parse(lines[4]),
                Timestamp = DateTime.Parse(lines[5])
            };
        }
        catch { return null; }
    }

    private static void SaveResult(BenchmarkResult r)
    {
        try
        {
            Directory.CreateDirectory(ResultDir);
            File.WriteAllLines(ResultFile, new[]
            {
                r.CpuMops.ToString(), r.RamMBps.ToString(),
                r.BgCpu.ToString(), r.BgRam.ToString(),
                r.Score.ToString(), r.Timestamp.ToString("O")
            });
        }
        catch { }
    }
}
