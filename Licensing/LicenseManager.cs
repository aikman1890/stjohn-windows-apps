// Per-app license enforcement: trial bookkeeping, stored-key validation,
// Saved-Work backup/restore, and the exit-from-lockout auto-uninstall.
//
// Everything here is OFFLINE. No network calls, no telemetry.
// Trial/key state lives in ApplicationData LocalSettings (values lightly
// obfuscated via LicenseCore.Obfuscate — not encryption, just anti-snoop).

using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Windows.Storage;

namespace StJohn.Licensing;

/// <summary>Per-app licensing configuration. Each app builds one of these.</summary>
public sealed class LicenseConfig
{
    /// <summary>Display name, e.g. "DayPilot". Used for folders, mail subjects.</summary>
    public required string AppName { get; init; }

    /// <summary>The product id this manager gates (base app access).</summary>
    public required ProductId Product { get; init; }

    /// <summary>Free-trial length in days from first launch. 0 = no trial.</summary>
    public int TrialDays { get; init; }

    /// <summary>Where the Buy button points.</summary>
    public required string BuyMailto { get; init; }

    /// <summary>Display price for the Buy button, e.g. "$2.99/month".</summary>
    public string PriceDisplay { get; init; } = string.Empty;

    public string SupportUrl { get; init; } = "https://aikman1890.github.io/stjohn-windows-apps/support.html";

    /// <summary>Returns the HMAC secret for <see cref="Product"/>.</summary>
    public required Func<byte[]> Secret { get; init; }
}

public enum AccessKind
{
    Licensed,
    Trial,
    Locked,
}

/// <summary>
/// Evaluates and enforces the license for one product. Construct one per
/// gated product (ClaimPilot uses three: base, Plus, Legal Matters).
/// </summary>
public sealed class LicenseManager
{
    private readonly LicenseConfig _config;

    private const string KeyFirstRun = "FirstRunUtcTicks";
    private const string KeyLastRun = "LastRunUtcTicks";
    private const string KeyLicense = "LicenseKey";
    private const string KeyLicenseEmail = "LicenseEmail";

    public LicenseManager(LicenseConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public LicenseConfig Config => _config;

    // ------------------------------------------------------------------
    // Access evaluation
    // ------------------------------------------------------------------

    /// <summary>
    /// Full evaluation: valid stored key -&gt; Licensed; else active trial
    /// -&gt; Trial; else Locked. Updates trial bookkeeping as a side effect.
    /// </summary>
    public AccessKind Evaluate(out string statusText, out int trialDaysLeft)
    {
        trialDaysLeft = 0;
        try
        {
            if (HasValidKey(out var email, out var expiry))
            {
                statusText = expiry is null
                    ? $"Licensed to {email} — {_config.Product} (perpetual)."
                    : $"Licensed to {email} — {_config.Product} valid through {expiry.Value:MMMM d, yyyy}.";
                return AccessKind.Licensed;
            }

            if (_config.TrialDays > 0 && CheckTrial(out trialDaysLeft, out var trialNote))
            {
                statusText = trialDaysLeft == 1
                    ? "Free trial: 1 day left."
                    : $"Free trial: {trialDaysLeft} days left.";
                if (!string.IsNullOrEmpty(trialNote))
                    statusText += " " + trialNote;
                return AccessKind.Trial;
            }

            statusText = LockedReason();
            return AccessKind.Locked;
        }
        catch
        {
            statusText = "License storage is unavailable — the app is locked.";
            return AccessKind.Locked;
        }
    }

    /// <summary>True when the app may run: a valid key or an active trial.</summary>
    public bool HasAccess() => Evaluate(out _, out _) != AccessKind.Locked;

    /// <summary>True when a stored key exists, verifies, and is not expired.</summary>
    public bool HasValidKey() => HasValidKey(out _, out _);

    public bool HasValidKey(out string email, out DateTime? expiry)
    {
        email = string.Empty;
        expiry = null;
        try
        {
            var c = Container;
            var keyText = LicenseCore.Deobfuscate(c.Values[KeyLicense] as string);
            var storedEmail = LicenseCore.Deobfuscate(c.Values[KeyLicenseEmail] as string);
            if (string.IsNullOrWhiteSpace(keyText))
                return false;
            if (!LicenseCore.TryParse(keyText, _config.Secret(), _config.Product, out var info, out _))
                return false;
            if (info is null || LicenseCore.IsExpired(info))
                return false;
            if (!string.IsNullOrEmpty(storedEmail) && !LicenseCore.EmailMatches(info, storedEmail))
                return false;
            email = storedEmail;
            expiry = info.ExpiryUtc;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a key the user typed, binds it to the given email, and stores it.
    /// </summary>
    public bool TryActivate(string? keyText, string? email, out string error)
    {
        error = string.Empty;
        email = (email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            error = "Enter the email address the key was issued for.";
            return false;
        }

        LicenseInfo? info;
        if (!LicenseCore.TryParse(keyText, _config.Secret(), _config.Product, out info, out error))
            return false;
        if (info is null)
        {
            error = "That license key is invalid.";
            return false;
        }
        if (LicenseCore.IsExpired(info))
        {
            error = $"That key expired on {info.ExpiryUtc:MMMM d, yyyy}. Renew to keep using {_config.AppName}.";
            return false;
        }
        if (!LicenseCore.EmailMatches(info, email))
        {
            error = "That key was issued for a different email address.";
            return false;
        }

        try
        {
            var c = Container;
            c.Values[KeyLicense] = LicenseCore.Obfuscate(keyText!.Trim());
            c.Values[KeyLicenseEmail] = LicenseCore.Obfuscate(email);
            return true;
        }
        catch
        {
            error = "The key is valid, but it could not be saved on this PC. Try again.";
            return false;
        }
    }

    /// <summary>Removes the stored key (used by Settings "sign out", if offered).</summary>
    public void Deactivate()
    {
        try
        {
            var c = Container;
            c.Values.Remove(KeyLicense);
            c.Values.Remove(KeyLicenseEmail);
        }
        catch { /* best effort */ }
    }

    public string GetStatusText()
    {
        Evaluate(out var text, out _);
        return text;
    }

    // ------------------------------------------------------------------
    // Trial bookkeeping (DayPilot only; TrialDays == 0 means no trial)
    // ------------------------------------------------------------------

    private ApplicationDataContainer Container =>
        ApplicationData.Current.LocalSettings.CreateContainer(
            "StJohnLicensing_" + _config.Product, ApplicationDataCreateDisposition.Always);

    private bool CheckTrial(out int daysLeft, out string note)
    {
        daysLeft = 0;
        note = string.Empty;
        var now = DateTime.UtcNow;
        var c = Container;

        var firstTicks = ParseTicks(LicenseCore.Deobfuscate(c.Values[KeyFirstRun] as string));
        var lastTicks = ParseTicks(LicenseCore.Deobfuscate(c.Values[KeyLastRun] as string));

        if (firstTicks is null || lastTicks is null)
        {
            // First launch: start the trial clock now.
            c.Values[KeyFirstRun] = LicenseCore.Obfuscate(now.Ticks.ToString());
            c.Values[KeyLastRun] = LicenseCore.Obfuscate(now.Ticks.ToString());
            daysLeft = _config.TrialDays;
            return true;
        }

        var lastRun = new DateTime(lastTicks.Value, DateTimeKind.Utc);
        if (now < lastRun)
        {
            // Clock was rolled back — treat the trial as expired.
            note = "The system clock appears to have been moved backwards, so the trial has ended.";
            return false;
        }

        c.Values[KeyLastRun] = LicenseCore.Obfuscate(now.Ticks.ToString());

        var firstRun = new DateTime(firstTicks.Value, DateTimeKind.Utc);
        var daysUsed = (int)(now.Date - firstRun.Date).TotalDays;
        daysLeft = _config.TrialDays - daysUsed;
        return daysLeft > 0;
    }

    private static long? ParseTicks(string s) =>
        long.TryParse(s, out var t) && t > 0 ? t : null;

    private string LockedReason()
    {
        if (_config.TrialDays > 0)
            return "Your free trial has ended.";
        return "A license is required to use " + _config.AppName + ".";
    }

    // ------------------------------------------------------------------
    // Saved Work: backup + restore (Documents\<App> Saved Work\)
    // ------------------------------------------------------------------

    /// <summary>Folder that survives uninstall: Documents\"{App} Saved Work".</summary>
    public string SavedWorkFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            $"{_config.AppName} Saved Work");

    /// <summary>
    /// Copies the app's LocalFolder into the Saved Work folder in readable
    /// form, and also drops a timestamped zip inside it. Never deletes.
    /// Returns the Saved Work folder path.
    /// </summary>
    public async Task<string> BackupSavedWorkAsync()
    {
        var destDir = SavedWorkFolder;
        var localPath = ApplicationData.Current.LocalFolder.Path;
        await Task.Run(() =>
        {
            Directory.CreateDirectory(destDir);
            CopyDirectory(localPath, destDir, excludeExtension: null);
            var zipName = $"{_config.AppName}-Backup-{DateTime.Now:yyyyMMdd-HHmmss}.zip";
            var zipPath = Path.Combine(destDir, zipName);
            if (File.Exists(zipPath))
                File.Delete(zipPath);
            ZipFile.CreateFromDirectory(localPath, zipPath, CompressionLevel.Fastest, includeBaseDirectory: false);
        });
        return destDir;
    }

    /// <summary>
    /// Copies saved work back into the app's LocalFolder (extracts any zips
    /// found, then copies loose files). Returns (filesCopied, zipsExtracted).
    /// </summary>
    public async Task<(int Files, int Zips)> RestoreSavedWorkAsync()
    {
        var srcDir = SavedWorkFolder;
        var localPath = ApplicationData.Current.LocalFolder.Path;
        return await Task.Run(() =>
        {
            if (!Directory.Exists(srcDir))
                return (0, 0);
            var zips = 0;
            foreach (var zip in Directory.GetFiles(srcDir, "*.zip"))
            {
                try
                {
                    ZipFile.ExtractToDirectory(zip, localPath, overwriteFiles: true);
                    zips++;
                }
                catch { /* one bad zip must not block the rest */ }
            }
            var files = CopyDirectory(srcDir, localPath, excludeExtension: ".zip");
            return (files, zips);
        });
    }

    /// <summary>Human-readable summary of what is in the Saved Work folder.</summary>
    public string GetSavedWorkSummary()
    {
        try
        {
            var dir = SavedWorkFolder;
            if (!Directory.Exists(dir))
                return "No saved work found in your Documents folder yet.";
            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            var zips = files.Count(f => f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
            var loose = files.Length - zips;
            if (files.Length == 0)
                return "The Saved Work folder exists but is empty.";
            return $"Found {loose} file(s)" + (zips > 0 ? $" and {zips} backup zip(s)" : "") +
                   $" in Documents\\{_config.AppName} Saved Work.";
        }
        catch
        {
            return "Could not read the Saved Work folder.";
        }
    }

    private static int CopyDirectory(string source, string dest, string? excludeExtension)
    {
        var copied = 0;
        Directory.CreateDirectory(dest);
        foreach (var file in Directory.GetFiles(source))
        {
            if (excludeExtension is not null &&
                file.EndsWith(excludeExtension, StringComparison.OrdinalIgnoreCase))
                continue;
            try
            {
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite: true);
                copied++;
            }
            catch { /* skip locked/unreadable files */ }
        }
        foreach (var dir in Directory.GetDirectories(source))
        {
            var name = Path.GetFileName(dir);
            if (string.IsNullOrEmpty(name))
                continue;
            copied += CopyDirectory(dir, Path.Combine(dest, name), excludeExtension);
        }
        return copied;
    }

    // ------------------------------------------------------------------
    // Auto-uninstall: the running app cannot delete its own folder, so we
    // drop a .cmd in %TEMP% that waits for our PID to exit, deletes the
    // install folder + shortcuts, then deletes itself.
    // ------------------------------------------------------------------

    /// <summary>
    /// Writes the cleanup script and launches it detached. Call only when the
    /// app is locked out and the user exits without activating a key.
    /// </summary>
    public void WriteUninstallScriptAndLaunch()
    {
        var pid = Environment.ProcessId;
        var installDir = AppContext.BaseDirectory
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Replace("\"", string.Empty);
        var scriptPath = Path.Combine(Path.GetTempPath(),
            $"{_config.AppName}-uninstall-{Guid.NewGuid():N}.cmd");

        var sb = new StringBuilder();
        sb.AppendLine("@echo off");
        sb.AppendLine("rem Auto-uninstall for " + _config.AppName + " (written by the app on lockout exit).");
        sb.AppendLine("setlocal");
        sb.AppendLine(":waitloop");
        sb.AppendLine($"tasklist /FI \"PID eq {pid}\" 2>NUL | find \"{pid}\" >NUL");
        sb.AppendLine("if %ERRORLEVEL%==0 ( timeout /t 1 /nobreak >NUL & goto waitloop )");
        sb.AppendLine($"rmdir /s /q \"{installDir}\"");
        sb.AppendLine($"del \"%APPDATA%\\Microsoft\\Windows\\Start Menu\\Programs\\{_config.AppName}.lnk\" 2>NUL");
        sb.AppendLine($"del \"%USERPROFILE%\\Desktop\\{_config.AppName}.lnk\" 2>NUL");
        sb.AppendLine("del \"%~f0\"");

        File.WriteAllText(scriptPath, sb.ToString(), Encoding.ASCII);
        Process.Start(new ProcessStartInfo
        {
            FileName = scriptPath,
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        });
    }
}
