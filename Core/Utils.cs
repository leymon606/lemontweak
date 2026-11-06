using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MoliTweak.Core;

public static class Utils
{
    private static readonly Logger Log = Logger.Instance;

    public static bool IsAdmin()
    {
        using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    public static (bool Success, string StdOut, string StdErr) RunCommand(string cmd, int timeoutMs = 20000)
    {
        try
        {
            var psi = new ProcessStartInfo("cmd.exe", $"/c {cmd}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            using var proc = Process.Start(psi)!;
            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();

            if (!proc.WaitForExit(timeoutMs))
            {
                proc.Kill();
                return (false, "", "Komut zaman aşımına uğradı.");
            }

            return (proc.ExitCode == 0, stdout.Trim(), stderr.Trim());
        }
        catch (Exception ex)
        {
            return (false, "", ex.Message);
        }
    }

    public static (bool Success, string StdOut, string StdErr) RunPowerShell(string script, int timeoutMs = 30000)
    {
        return RunCommand($"powershell.exe -NoProfile -ExecutionPolicy Bypass -Command \"{script}\"", timeoutMs);
    }

    public static bool SetRegistryValue(RegistryKey root, string subKeyPath, string name, RegistryValueKind kind, object value)
    {
        try
        {
            using var key = root.CreateSubKey(subKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree,
                RegistryOptions.None);
            if (key == null) return false;
            key.SetValue(name, value, kind);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            Log.Error($"Kayıt defteri yazma izni reddedildi: {subKeyPath}\\{name}");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error($"Kayıt defteri hatası [{subKeyPath}\\{name}]: {ex.Message}");
            return false;
        }
    }

    public static object? GetRegistryValue(RegistryKey root, string subKeyPath, string name)
    {
        try
        {
            using var key = root.OpenSubKey(subKeyPath);
            return key?.GetValue(name);
        }
        catch { return null; }
    }

    public static bool DeleteRegistryValue(RegistryKey root, string subKeyPath, string name)
    {
        try
        {
            using var key = root.OpenSubKey(subKeyPath, writable: true);
            if (key == null) return true;
            key.DeleteValue(name, throwOnMissingValue: false);
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning($"Kayıt defteri değeri silinemedi [{subKeyPath}\\{name}]: {ex.Message}");
            return false;
        }
    }

    public static bool DeleteRegistryKey(RegistryKey root, string subKeyPath)
    {
        try
        {
            root.DeleteSubKeyTree(subKeyPath, throwOnMissingSubKey: false);
            return true;
        }
        catch { return false; }
    }

    public static string? GetFiveMDirectory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string[] candidates =
        [
            Path.Combine(localAppData, "FiveM", "FiveM.app"),
            Path.Combine(localAppData, "FiveM"),
            Path.Combine(appData, "CitizenFX")
        ];
        foreach (var p in candidates)
            if (Directory.Exists(p)) return p;
        return null;
    }

    public static string? GetGtaSettingsFile()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string[] candidates =
        [
            Path.Combine(userProfile, "Documents", "Rockstar Games", "GTA V", "settings.xml"),
            Path.Combine(userProfile, "OneDrive", "Documents", "Rockstar Games", "GTA V", "settings.xml"),
            Path.Combine(appData, "CitizenFX", "gta5_settings.xml")
        ];
        foreach (var p in candidates)
            if (File.Exists(p)) return p;
        return null;
    }

    public static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "0 B";
        string[] names = ["B", "KB", "MB", "GB", "TB"];
        int i = 0;
        double size = bytes;
        while (size >= 1024 && i < names.Length - 1) { size /= 1024.0; i++; }
        return $"{size:F2} {names[i]}";
    }
}
