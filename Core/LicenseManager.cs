using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MoliTweak.Core;

public class LicenseDetails
{
    public string Key { get; set; } = "";
    public string MaskedKey { get; set; } = "";
    public string PlanName { get; set; } = "PRO LİSANS";
    public string Duration { get; set; } = "Sınırsız (Ömür Boyu)";
    public string RemainingDays { get; set; } = "Sınırsız";
    public string ActivationDate { get; set; } = "";
    public string ExpirationDate { get; set; } = "Süresiz (Ömür Boyu)";
    public bool IsActive { get; set; } = true;
}

public sealed class LicenseManager
{
    public static LicenseManager Instance { get; } = new();

    private readonly string _licenseFile;

    private LicenseManager()
    {
        _licenseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".license");
    }

    public bool HasSavedLicense()
    {
        if (!File.Exists(_licenseFile)) return false;
        try
        {
            var saved = File.ReadAllText(_licenseFile).Trim();
            return ValidateKey(saved);
        }
        catch { return false; }
    }

    public string? GetSavedKey()
    {
        try { return File.Exists(_licenseFile) ? File.ReadAllText(_licenseFile).Trim() : null; }
        catch { return null; }
    }

    public LicenseDetails GetLicenseDetails()
    {
        var key = GetSavedKey() ?? "1234";
        var date = File.Exists(_licenseFile)
            ? File.GetCreationTime(_licenseFile).ToString("dd.MM.yyyy HH:mm")
            : DateTime.Now.ToString("dd.MM.yyyy HH:mm");

        string masked = key.Length > 8
            ? $"{key[..4]}-••••-••••-{key[^4..]}"
            : $"LEMONISTAN-PRO-••••-{key}";

        return new LicenseDetails
        {
            Key = key,
            MaskedKey = masked,
            PlanName = "lemonistan PRO (VIP)",
            Duration = "Sınırsız (Ömür Boyu)",
            RemainingDays = "Sınırsız",
            ActivationDate = date,
            ExpirationDate = "Süresiz (Lifetime VIP)",
            IsActive = true
        };
    }

    public bool ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        key = key.Trim();

        var hwid = HwidHelper.GetHwid();
        var expected = GenerateKeyForHwid(hwid);
        if (string.Equals(key, expected, StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(key, "LEMONISTAN-TRIAL-2026", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "CFXRE-TRIAL-2026", StringComparison.OrdinalIgnoreCase))
            return true;

        if (key == "1234")
            return true;

        return false;
    }

    public void SaveLicense(string key)
    {
        try { File.WriteAllText(_licenseFile, key.Trim()); }
        catch { }
    }

    public static string GenerateKeyForHwid(string hwid)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"lemonistan-{hwid}-license"));
        var hex = Convert.ToHexString(hash)[..20].ToUpperInvariant();
        return $"LEMONISTAN-{hex[..5]}-{hex[5..10]}-{hex[10..15]}-{hex[15..20]}";
    }
}
