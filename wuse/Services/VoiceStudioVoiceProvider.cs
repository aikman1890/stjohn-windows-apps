using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Windows.Globalization;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace Wuse.Services;

/// <summary>
/// Voice provider that speaks through Shaun's own cloned voice, served by
/// VoiceStudio running locally on this PC (OpenAI-compatible API at
/// http://localhost:3900/v1).
///
/// Shaun wants his voice ALWAYS: if the API isn't answering, the provider
/// tries to start VoiceStudio itself from its known install locations
/// (per-user install, machine install, then the Start-menu shortcut) and
/// waits up to ~20 seconds for it to come up before giving up.
///
/// Everything is defensive: failures return false so the caller can fall back
/// to the system voice. Nothing here ever throws out of the public methods.
/// </summary>
public sealed class VoiceStudioVoiceProvider : IVoiceProvider
{
    private const string BaseUrl = "http://localhost:3900/v1";
    // Health lives at the service root, not under /v1 (per VoiceStudio docs).
    private const string HealthUrl = "http://localhost:3900/health";
    private const int HealthPollIntervalMs = 2000;
    private const int HealthPollAttempts = 10; // ~20 seconds of waiting

    private static readonly Lazy<HttpClient> SharedClient = new(() =>
    {
        var handler = new SocketsHttpHandler
        {
            // Fail fast when VoiceStudio isn't listening; speech generation
            // itself gets the full request timeout below.
            ConnectTimeout = TimeSpan.FromSeconds(3),
        };
        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };
    });

    private readonly Func<string?> _profileNameResolver;
    private readonly Action<string>? _notify;
    private readonly object _launchGate = new();
    private MediaPlayer? _player;
    private bool _launchAttempted;

    /// <param name="profileNameResolver">
    /// Returns the VoiceStudio voice-profile name to use (read at speak time
    /// so Settings changes apply immediately). Empty/null disables this
    /// provider.
    /// </param>
    /// <param name="notify">
    /// Optional callback for user-facing notices (e.g. "Starting VoiceStudio
    /// for your voice…"). Never throws back into the provider.
    /// </param>
    public VoiceStudioVoiceProvider(Func<string?> profileNameResolver, Action<string>? notify = null)
    {
        _profileNameResolver = profileNameResolver;
        _notify = notify;
    }

    /// <summary>
    /// Tries to speak <paramref name="text"/> through the VoiceStudio clone.
    /// Returns true when audio was produced and playback started; false when
    /// the caller should fall back to another voice. Never throws.
    /// </summary>
    /// <param name="language">
    /// BCP-47 language tag for cross-lingual speech. When null/empty, the
    /// app's current UI language is used so his voice speaks the app's
    /// language.
    /// </param>
    public async Task<bool> TrySpeakAsync(string text, string? language = null)
    {
        var profile = (_profileNameResolver() ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(profile) || string.IsNullOrWhiteSpace(text))
            return false;

        try
        {
            var result = await TryPostSpeechAsync(profile, text, language).ConfigureAwait(true);
            if (result == SpeechResult.Spoken)
                return true;

            // The API didn't answer. If VoiceStudio simply isn't running,
            // try to start it (once per session) before giving up — Shaun
            // wants his voice, not a generic one.
            bool shouldLaunch;
            lock (_launchGate)
            {
                shouldLaunch = result == SpeechResult.ConnectionFailed && !_launchAttempted;
                if (shouldLaunch)
                    _launchAttempted = true;
            }
            if (shouldLaunch)
            {
                try { _notify?.Invoke("Starting VoiceStudio for your voice…"); }
                catch { /* a notice must never break speech */ }

                if (await EnsureVoiceStudioRunningAsync().ConfigureAwait(true)
                    && await TryPostSpeechAsync(profile, text, language).ConfigureAwait(true) == SpeechResult.Spoken)
                {
                    lock (_launchGate) { _launchAttempted = false; } // it worked; a future outage may retry
                    return true;
                }
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>IVoiceProvider entry point: best-effort, never throws.</summary>
    public async Task SpeakAsync(string text)
    {
        try { await TrySpeakAsync(text).ConfigureAwait(true); }
        catch (Exception) { }
    }

    public void Stop()
    {
        try
        {
            _player?.Pause();
            _player?.Dispose();
        }
        catch (Exception)
        {
        }
        finally
        {
            _player = null;
        }
    }

    private enum SpeechResult
    {
        Spoken,
        /// <summary>Nothing is listening on localhost:3900 — maybe startable.</summary>
        ConnectionFailed,
        /// <summary>Service answered but the request failed (bad profile, etc.).</summary>
        Failed,
    }

    private async Task<SpeechResult> TryPostSpeechAsync(string profile, string text, string? language)
    {
        try
        {
            Stop(); // cut any current playback before starting new speech

            var payload = new Dictionary<string, object?>
            {
                ["model"] = "tts-1",
                ["voice"] = profile,
                ["input"] = text,
                ["response_format"] = "wav",
            };
            var lang = ResolveLanguage(language);
            if (!string.IsNullOrEmpty(lang))
                payload["language"] = lang;

            using var content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var response = await SharedClient.Value
                .PostAsync(BaseUrl + "/audio/speech", content)
                .ConfigureAwait(true);

            if (!response.IsSuccessStatusCode)
                return SpeechResult.Failed; // service is up; launching again won't help

            var wav = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(true);
            if (!IsWav(wav))
                return SpeechResult.Failed;

            await PlayWavAsync(wav).ConfigureAwait(true);
            return SpeechResult.Spoken;
        }
        catch (HttpRequestException)
        {
            // Connection refused / connect timeout / DNS: VoiceStudio isn't listening.
            return SpeechResult.ConnectionFailed;
        }
        catch (Exception)
        {
            // Includes the 60s generation timeout: the service is up but slow.
            return SpeechResult.Failed;
        }
    }

    private async Task PlayWavAsync(byte[] wav)
    {
        Stop();
        var stream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(stream))
        {
            writer.WriteBytes(wav);
            await writer.StoreAsync();
            writer.DetachStream();
        }
        stream.Seek(0);
        // MediaPlayer must be created on the UI thread; callers are page
        // code-behind handlers, which run on the UI thread, and we never
        // use ConfigureAwait(false) above.
        _player = new MediaPlayer();
        _player.Source = MediaSource.CreateFromStream(stream, "audio/wav");
        _player.Play();
    }

    private static bool IsWav(byte[] bytes)
    {
        return bytes.Length >= 12
            && bytes[0] == (byte)'R' && bytes[1] == (byte)'I'
            && bytes[2] == (byte)'F' && bytes[3] == (byte)'F'
            && bytes[8] == (byte)'W' && bytes[9] == (byte)'A'
            && bytes[10] == (byte)'V' && bytes[11] == (byte)'E';
    }

    private static string ResolveLanguage(string? language)
    {
        if (!string.IsNullOrWhiteSpace(language))
            return language.Trim();
        try
        {
            var langs = ApplicationLanguages.Languages;
            if (langs != null && langs.Count > 0 && !string.IsNullOrWhiteSpace(langs[0]))
                return langs[0];
        }
        catch (Exception)
        {
            // Fall through to CurrentUICulture.
        }
        try
        {
            return System.Globalization.CultureInfo.CurrentUICulture.Name;
        }
        catch (Exception)
        {
            return "en-US";
        }
    }

    private static async Task<bool> IsHealthyAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            using var response = await SharedClient.Value
                .GetAsync(HealthUrl, cts.Token)
                .ConfigureAwait(true);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Makes sure the VoiceStudio API is up, launching the app if needed.
    /// Returns true when /health answers.
    /// </summary>
    private static async Task<bool> EnsureVoiceStudioRunningAsync()
    {
        try
        {
            if (await IsHealthyAsync().ConfigureAwait(true))
                return true; // it came up between our attempts

            var target = FindVoiceStudioExe();
            if (target == null)
                return false;

            try
            {
                // Works for both the .exe and a Start-menu .lnk (Windows
                // resolves the shortcut itself).
                Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true });
            }
            catch (Exception)
            {
                return false;
            }

            for (int i = 0; i < HealthPollAttempts; i++)
            {
                await Task.Delay(HealthPollIntervalMs).ConfigureAwait(true);
                if (await IsHealthyAsync().ConfigureAwait(true))
                    return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Known VoiceStudio install locations, in order: per-user install,
    /// machine install, then a Start-menu shortcut lookup.
    /// </summary>
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
        catch (Exception)
        {
            // Fall through to the shortcut search.
        }
        return FindVoiceStudioShortcut();
    }

    private static string? FindVoiceStudioShortcut()
    {
        var roots = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"),
        };
        foreach (var root in roots)
        {
            if (!Directory.Exists(root))
                continue;
            try
            {
                foreach (var file in Directory.EnumerateFiles(root, "*VoiceStudio*.lnk", SearchOption.AllDirectories))
                    return file; // first match wins
            }
            catch (Exception)
            {
                // Inaccessible corners of the Start menu are skipped silently.
            }
        }
        return null;
    }
}
