using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Windows.Storage;

namespace DayPilot.Services;

/// <summary>
/// First-launch voice-engine setup: makes sure VoiceStudio (the free local
/// app that speaks in Shaun's cloned voice) is installed, running, and has
/// Shaun's "shaun2" voice profile imported — with zero manual steps from the
/// user. Everything is best-effort: any failure simply leaves the Windows
/// system voice in place. Nothing here ever throws.
///
/// Flow: voice-bundle integrity is verified by IsVoiceIntact() (called from
/// the startup tamper gate). EnsureAsync() then: (1) checks the VoiceStudio
/// API; (2) if missing, downloads the latest per-user installer from the
/// VoiceStudio GitHub releases and runs it silently (/S, no admin);
/// (3) launches VoiceStudio and waits for its API; (4) imports the decrypted
/// voice bundle via POST /personas/import; (5) verifies the profile appears
/// in GET /v1/audio/voices. A LocalSettings flag makes it run once.
/// </summary>
public static class VoiceEngineBootstrap
{
    private const string DoneFlag = "VoiceEngineBootstrapped";
    private const string VoiceTamperedFlag = "VoiceTampered";
    private const string ProfileName = "shaun2";
    private const string ReleasesApi = "https://api.github.com/repos/debpalash/VoiceStudio/releases/latest";
    private const string HealthUrl = "http://localhost:3900/health";
    private const string ImportUrl = "http://localhost:3900/personas/import";
    private const string VoicesUrl = "http://localhost:3900/v1/audio/voices";

    private static readonly TimeSpan HttpTimeout = TimeSpan.FromMinutes(10);
    private const int HealthPolls = 300;      // ~10 minutes for first-run model setup
    private const int HealthPollMs = 2000;

    /// <summary>
    /// Verifies the encrypted voice bundle. False = tampered: the caller
    /// must show the tamper lockout screen. Never throws.
    /// </summary>
    public static bool IsVoiceIntact()
    {
        try
        {
            var plain = VoiceCrypto.DecryptAndVerify(out var actualPrefix8);
            if (plain == null)
            {
                try { ApplicationData.Current.LocalSettings.Values[VoiceTamperedFlag] = true; }
                catch { }
                IntegrityGuard.WriteTamperLog("voice bundle hash mismatch (expected " +
                    VoiceCrypto.ExpectedHashPrefix8 + ", got " + actualPrefix8 + ")");
                return false;
            }
            Array.Clear(plain, 0, plain.Length);
            try { ApplicationData.Current.LocalSettings.Values.Remove(VoiceTamperedFlag); }
            catch { }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Fire-and-forget from app startup. Never throws.</summary>
    public static async Task EnsureAsync()
    {
        try
        {
            if (IsDone())
                return;

            if (!await IsHealthyAsync().ConfigureAwait(false))
            {
                if (!IsInstalled() && !await DownloadAndInstallAsync().ConfigureAwait(false))
                    return; // fail soft — the system voice stays
                LaunchApp();
                if (!await WaitForHealthyAsync().ConfigureAwait(false))
                    return; // will retry on next launch (flag not set)
            }

            // Profile already there (e.g. Shaun's own copy)? Nothing to do.
            if (await HasProfileAsync().ConfigureAwait(false))
            {
                MarkDone();
                return;
            }

            var plain = VoiceCrypto.DecryptAndVerify();
            if (plain == null)
                return; // tamper path is handled by the startup gate
            try
            {
                if (await ImportProfileAsync(plain).ConfigureAwait(false)
                    && await HasProfileAsync().ConfigureAwait(false))
                    MarkDone();
            }
            finally
            {
                Array.Clear(plain, 0, plain.Length);
            }
        }
        catch
        {
        }
    }

    private static bool IsDone()
    {
        try { return ApplicationData.Current.LocalSettings.Values[DoneFlag] is true; }
        catch { return false; }
    }

    private static void MarkDone()
    {
        try { ApplicationData.Current.LocalSettings.Values[DoneFlag] = true; }
        catch { }
    }

    private static async Task<bool> IsHealthyAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var http = new HttpClient();
            using var resp = await http.GetAsync(HealthUrl, cts.Token).ConfigureAwait(false);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> WaitForHealthyAsync()
    {
        for (int i = 0; i < HealthPolls; i++)
        {
            if (await IsHealthyAsync().ConfigureAwait(false))
                return true;
            await Task.Delay(HealthPollMs).ConfigureAwait(false);
        }
        return false;
    }

    private static bool IsInstalled()
    {
        return FindVoiceStudioExe() != null;
    }

    private static string? FindVoiceStudioExe()
    {
        try
        {
            var perUser = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VoiceStudio (Current User)", "VoiceStudio.exe");
            if (File.Exists(perUser))
                return perUser;

            var machine = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "VoiceStudio", "VoiceStudio.exe");
            if (File.Exists(machine))
                return machine;
        }
        catch
        {
        }
        return null;
    }

    private static void LaunchApp()
    {
        try
        {
            var exe = FindVoiceStudioExe();
            if (exe != null)
                Process.Start(new ProcessStartInfo { FileName = exe, UseShellExecute = true });
        }
        catch
        {
        }
    }

    /// <summary>
    /// Downloads the latest per-user VoiceStudio installer from the GitHub
    /// releases API and runs it silently (NSIS /S — current user, no admin).
    /// Asset-name matching tolerates renames; anything unexpected fails soft.
    /// </summary>
    private static async Task<bool> DownloadAndInstallAsync()
    {
        try
        {
            using var http = new HttpClient { Timeout = HttpTimeout };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("stjohn-windows-apps/1.0");

            string? assetUrl = null;
            using (var rel = await http.GetAsync(ReleasesApi).ConfigureAwait(false))
            {
                if (!rel.IsSuccessStatusCode)
                    return false;
                using var doc = JsonDocument.Parse(
                    await rel.Content.ReadAsStringAsync().ConfigureAwait(false));
                if (!doc.RootElement.TryGetProperty("assets", out var assets))
                    return false;
                foreach (var a in assets.EnumerateArray())
                {
                    if (!a.TryGetProperty("name", out var n) ||
                        !a.TryGetProperty("browser_download_url", out var u))
                        continue;
                    var name = n.GetString() ?? string.Empty;
                    if (name.StartsWith("VoiceStudio-Electron-", StringComparison.OrdinalIgnoreCase)
                        && name.Contains("-win-x64", StringComparison.OrdinalIgnoreCase)
                        && name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        assetUrl = u.GetString();
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(assetUrl))
                return false;

            var tmp = Path.Combine(Path.GetTempPath(), "VoiceStudio-Setup.exe");
            using (var resp = await http.GetAsync(assetUrl, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false))
            {
                if (!resp.IsSuccessStatusCode)
                    return false;
                await using var fs = File.Create(tmp);
                await resp.Content.CopyToAsync(fs).ConfigureAwait(false);
            }

            try
            {
                using var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = tmp,
                    Arguments = "/S",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                });
                if (proc == null)
                    return false;
                if (!proc.WaitForExit((int)TimeSpan.FromMinutes(20).TotalMilliseconds))
                    return false;
                if (proc.ExitCode != 0)
                    return false;
            }
            finally
            {
                try { File.Delete(tmp); } catch { }
            }

            return IsInstalled();
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> HasProfileAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var http = new HttpClient();
            using var resp = await http.GetAsync(VoicesUrl, cts.Token).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
                return false;
            using var doc = JsonDocument.Parse(
                await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
            return JsonContainsProfileName(doc.RootElement);
        }
        catch
        {
            return false;
        }
    }

    private static bool JsonContainsProfileName(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                string? name = null;
                foreach (var p in el.EnumerateObject())
                {
                    if (p.NameEquals("name") && p.Value.ValueKind == JsonValueKind.String)
                        name = p.Value.GetString();
                    else if (JsonContainsProfileName(p.Value))
                        return true;
                }
                return name != null &&
                    name.Equals(ProfileName, StringComparison.OrdinalIgnoreCase);
            case JsonValueKind.Array:
                foreach (var item in el.EnumerateArray())
                    if (JsonContainsProfileName(item))
                        return true;
                return false;
            default:
                return false;
        }
    }

    /// <summary>
    /// Imports the decrypted .ovsvoice bundle through the backend's persona
    /// import endpoint. The plaintext only ever lives in memory.
    /// </summary>
    private static async Task<bool> ImportProfileAsync(byte[] ovsvoice)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            using var form = new MultipartFormDataContent();
            using var content = new ByteArrayContent(ovsvoice);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            form.Add(content, "file", "shaun2.ovsvoice");
            using var resp = await http.PostAsync(ImportUrl, form).ConfigureAwait(false);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
