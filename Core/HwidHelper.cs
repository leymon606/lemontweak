using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace MoliTweak.Core;

public static class HwidHelper
{
    private static string? _cached;

    public static string GetHwid()
    {
        if (_cached != null) return _cached;

        var raw = new StringBuilder();
        raw.Append(GetWmiValue("Win32_Processor", "ProcessorId"));
        raw.Append(GetWmiValue("Win32_BaseBoard", "SerialNumber"));
        raw.Append(GetWmiValue("Win32_DiskDrive", "SerialNumber"));
        raw.Append(Environment.MachineName);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw.ToString()));
        _cached = Convert.ToHexString(hash)[..16].ToLowerInvariant();
        return _cached;
    }

    private static string GetWmiValue(string wmiClass, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}");
            foreach (var obj in searcher.Get())
            {
                var val = obj[property]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(val)) return val;
            }
        }
        catch { }
        return "unknown";
    }
}
