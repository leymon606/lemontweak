using System;
using System.Collections.Generic;
using System.IO;

namespace MoliTweak.Core;

public enum LogLevel { INFO, SUCCESS, WARNING, ERROR }

public class LogEventArgs : EventArgs
{
    public string Message { get; }
    public LogLevel Level { get; }
    public LogEventArgs(string message, LogLevel level) { Message = message; Level = level; }
}

public sealed class Logger
{
    public static Logger Instance { get; } = new();

    public event EventHandler<LogEventArgs>? LogReceived;

    private readonly string _logFilePath;
    private readonly object _lock = new();

    private Logger()
    {
        var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(logDir);
        _logFilePath = Path.Combine(logDir, $"optimizer_{DateTime.Now:yyyyMMdd_HHmmss}.log");
    }

    public void Log(string message, LogLevel level = LogLevel.INFO)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var formatted = $"[{timestamp}] [{level}] {message}";

        lock (_lock)
        {
            try { File.AppendAllText(_logFilePath, formatted + Environment.NewLine); }
            catch { }
        }

        LogReceived?.Invoke(this, new LogEventArgs(formatted, level));
    }

    public void Info(string msg) => Log(msg, LogLevel.INFO);
    public void Success(string msg) => Log(msg, LogLevel.SUCCESS);
    public void Warning(string msg) => Log(msg, LogLevel.WARNING);
    public void Error(string msg) => Log(msg, LogLevel.ERROR);
}
