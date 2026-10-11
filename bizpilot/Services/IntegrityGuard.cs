using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace BizPilot.Services;

/// <summary>
/// Startup binary-integrity guard. In Release builds it verifies the
/// HMAC-SHA256-signed integrity manifest (integrity.manifest, generated and
/// signed at build time by .github/workflows/build-and-publish.yml) and then
/// re-hashes every listed file. Debug builds skip the check entirely so the
/// dev loop never breaks.
///
/// Any failure — bad signature, hash mismatch, missing file, missing manifest
/// — means the installation was tampered with: the caller must show the
/// tamper lockout screen instead of the app. This class never throws; it only
/// reports. Failures are appended to tamper.log in the app's LocalFolder.
/// </summary>
public static class IntegrityGuard
{
    private const string ManifestName = "integrity.manifest";

    // HMAC key: masked and split across 8 literals so the key never appears
    // as one readable value in the shipped binary. It must match the
    // INTEGRITY_KEY repo secret used to sign the manifest at build time.
    private static readonly byte[] HK1 = new byte[] { 0x74, 0xF9, 0x6C, 0x63, 0x8D, 0xF6, 0xD7, 0x8D };
    private static readonly byte[] HK2 = new byte[] { 0x3C, 0x79, 0x3E, 0xA0, 0x93, 0x30, 0x6B, 0xCD };
    private static readonly byte[] HK3 = new byte[] { 0xA2, 0x01, 0xBD, 0x59, 0x6A, 0xA1, 0xA5, 0x60 };
    private static readonly byte[] HK4 = new byte[] { 0x04, 0x97, 0x0D, 0x6C, 0x47, 0x6D, 0x96, 0xA5 };
    private static readonly byte[] HM1 = new byte[] { 0x1B, 0x03, 0x9E, 0xC0, 0x14, 0xD8, 0xF6, 0xBD };
    private static readonly byte[] HM2 = new byte[] { 0xB5, 0x65, 0x00, 0x2E, 0x74, 0x59, 0xC0, 0x50 };
    private static readonly byte[] HM3 = new byte[] { 0x0D, 0x73, 0x8A, 0x5B, 0xE5, 0xCD, 0xDC, 0xA7 };
    private static readonly byte[] HM4 = new byte[] { 0xDA, 0x4E, 0x4B, 0x5B, 0x98, 0xB4, 0x67, 0xA4 };

    private static byte[] HKey()
    {
        var k = new byte[32];
        var m = new byte[32];
        Buffer.BlockCopy(HK1, 0, k, 0, 8);
        Buffer.BlockCopy(HK2, 0, k, 8, 8);
        Buffer.BlockCopy(HK3, 0, k, 16, 8);
        Buffer.BlockCopy(HK4, 0, k, 24, 8);
        Buffer.BlockCopy(HM1, 0, m, 0, 8);
        Buffer.BlockCopy(HM2, 0, m, 8, 8);
        Buffer.BlockCopy(HM3, 0, m, 16, 8);
        Buffer.BlockCopy(HM4, 0, m, 24, 8);
        for (int i = 0; i < 32; i++) k[i] ^= m[i];
        Array.Clear(m, 0, m.Length);
        return k;
    }

    /// <summary>
    /// True when the installation shows any sign of tampering. Never throws.
    /// </summary>
    public static bool IsTampered()
    {
#if DEBUG
        return false;
#else
        try
        {
            var dir = AppContext.BaseDirectory;
            var manifestPath = Path.Combine(dir, ManifestName);
            if (!File.Exists(manifestPath))
            {
                WriteTamperLog("integrity manifest missing");
                return true;
            }

            var lines = File.ReadAllLines(manifestPath);
            if (lines.Length < 2)
            {
                WriteTamperLog("integrity manifest malformed");
                return true;
            }

            var sigLine = lines[lines.Length - 1];
            if (!sigLine.StartsWith("hmac:", StringComparison.Ordinal))
            {
                WriteTamperLog("integrity manifest not signed");
                return true;
            }

            var body = string.Join("\n", lines, 0, lines.Length - 1) + "\n";
            var expected = sigLine.Substring(5).Trim().ToLowerInvariant();
            string actual;
            var key = HKey();
            try
            {
                using var hmac = new HMACSHA256(key);
                actual = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
            }

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(actual), Encoding.ASCII.GetBytes(expected)))
            {
                WriteTamperLog("manifest signature mismatch");
                return true;
            }

            using var sha = SHA256.Create();
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var line = lines[i];
                var sep = line.IndexOf("  ", StringComparison.Ordinal);
                if (sep <= 0)
                {
                    WriteTamperLog("manifest line malformed");
                    return true;
                }
                var want = line.Substring(0, sep).Trim().ToLowerInvariant();
                var rel = line.Substring(sep + 2).Trim();
                if (rel.Contains("..", StringComparison.Ordinal))
                {
                    WriteTamperLog("manifest path escape rejected");
                    return true;
                }
                var full = Path.Combine(dir, rel);
                if (!File.Exists(full))
                {
                    WriteTamperLog("installed file missing: " + rel);
                    return true;
                }
                var bytes = File.ReadAllBytes(full);
                string got;
                try
                {
                    got = Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
                }
                finally
                {
                    Array.Clear(bytes, 0, bytes.Length);
                }
                if (!CryptographicOperations.FixedTimeEquals(
                        Encoding.ASCII.GetBytes(got), Encoding.ASCII.GetBytes(want)))
                {
                    WriteTamperLog("file hash mismatch: " + rel +
                        " (expected " + want.Substring(0, 12) + ", got " + got.Substring(0, 12) + ")");
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            WriteTamperLog("integrity check failed: " + ex.GetType().Name);
            return true;
        }
#endif
    }

    /// <summary>Replaces the window content with the tamper lockout page.</summary>
    public static void ShowLockout(Window window)
    {
        try
        {
            window.Content = new Views.TamperLockPage();
        }
        catch
        {
            // If even the lockout page can't load, the window stays blank —
            // the app still must not proceed.
        }
    }

    /// <summary>
    /// Appends a tamper event (UTC timestamp, reason, app version) to
    /// tamper.log in the app's LocalFolder. Never throws.
    /// </summary>
    public static void WriteTamperLog(string reason)
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder.Path;
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            var line = string.Format("{0:O} | {1} | v{2}{3}",
                DateTime.UtcNow, reason, version, Environment.NewLine);
            File.AppendAllText(Path.Combine(folder, "tamper.log"), line);
        }
        catch
        {
        }
    }
}
