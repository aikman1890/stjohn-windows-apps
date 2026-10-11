using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;

namespace Wuse.Services;

/// <summary>
/// Swappable voice-output seam. The default implementation speaks through the
/// built-in Windows voices; a future provider (for example a cloned custom
/// voice) can replace it without touching any callers — just assign a new
/// <see cref="SpeechService.VoiceProvider"/>.
/// </summary>
public interface IVoiceProvider
{
    Task SpeakAsync(string text);
    void Stop();
}

/// <summary>
/// Default voice provider: Windows system voices via SSML with a calm, warm,
/// friendly male delivery — gently unhurried, never sleepy, never robotic.
/// </summary>
public sealed class SystemVoiceProvider : IVoiceProvider
{
    private readonly Func<VoiceInformation?> _voiceSelector;
    private MediaPlayer? _player;

    // Calm AND friendly prosody: slightly under normal rate so it feels
    // unhurried, pitch just a touch below medium, friendly volume.
    private const string SsmlRate = "0.92";
    private const string SsmlPitch = "-2%";
    private const string SsmlVolume = "medium";

    public SystemVoiceProvider(Func<VoiceInformation?> voiceSelector)
    {
        _voiceSelector = voiceSelector;
    }

    public async Task SpeakAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        try
        {
            Stop();

            using var synthesizer = new SpeechSynthesizer();
            var voice = _voiceSelector();
            if (voice != null)
            {
                try { synthesizer.Voice = voice; }
                catch (Exception) { /* fall back to the default voice */ }
            }

            string ssml =
                "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\">" +
                "<prosody rate=\"" + SsmlRate + "\" pitch=\"" + SsmlPitch + "\" volume=\"" + SsmlVolume + "\">" +
                EscapeSsml(text) +
                "</prosody></speak>";

            var stream = await synthesizer.SpeakSsmlAsync(ssml);

            // MediaPlayer must be created on the UI thread; callers are
            // page code-behind handlers, which run on the UI thread.
            _player = new MediaPlayer();
            _player.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
            _player.Play();
        }
        catch (Exception)
        {
            // Speech output is a nice-to-have; the on-screen text carries
            // the message either way.
        }
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

    private static string EscapeSsml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }
}

/// <summary>
/// Shared speech engine for wuse: microphone speech-to-text (SpeechRecognizer,
/// dictation grammar) and text-to-speech through the swappable
/// <see cref="IVoiceProvider"/>.
///
/// Everything is defensive: every speech API call is wrapped in try/catch and
/// surfaces a friendly message instead of throwing, because microphones,
/// speech engines, and voices vary wildly across PCs.
/// </summary>
public sealed class SpeechService
{
    private static readonly Lazy<SpeechService> _instance = new(() => new SpeechService());

    public static SpeechService Instance => _instance.Value;

    private const string VoiceSettingKey = "WuseVoiceId";

    // Warm male voices we prefer, in order (friendly neural voices first).
    private static readonly string[] PreferredMaleVoiceHints =
    {
        "guy", "davis", "tony", "christopher", "david", "mark",
        "eric", "jacob", "brandon", "ryan", "jason",
    };

    private VoiceInformation? _selectedVoice;

    private readonly SystemVoiceProvider _systemProvider;
    private readonly VoiceStudioVoiceProvider _voiceStudioProvider;
    private readonly SettingsService _settings = new();

    private SpeechService()
    {
        _selectedVoice = LoadSavedVoice() ?? PickDefaultVoice();
        // The selector reads the field at speak time, so voice changes
        // (Settings picker) take effect immediately.
        _systemProvider = new SystemVoiceProvider(() => _selectedVoice);
        VoiceProvider = _systemProvider;
        // Shaun's cloned voice: tried first whenever a profile name is set
        // (default "Shaun"). Falls back to the system voice on any failure.
        _voiceStudioProvider = new VoiceStudioVoiceProvider(
            () => _settings.VoiceStudioProfile,
            notice => { try { VoiceNotice?.Invoke(notice); } catch { } });
    }

    /// <summary>
    /// The fallback voice output (Windows system voices). Still swappable,
    /// but Shaun's VoiceStudio clone is now tried first by <see cref="SpeakAsync"/>.
    /// </summary>
    public IVoiceProvider VoiceProvider { get; set; }

    /// <summary>
    /// Fires with user-facing notices from the voice layer — e.g.
    /// "Starting VoiceStudio for your voice…". UI may subscribe and show them.
    /// </summary>
    public event Action<string>? VoiceNotice;

    /// <summary>All voices installed on this PC (for the Settings picker).</summary>
    public IReadOnlyList<VoiceInformation> AvailableVoices => SpeechSynthesizer.AllVoices;

    public VoiceInformation? SelectedVoice
    {
        get => _selectedVoice;
        set
        {
            if (value == null || value.Id == _selectedVoice?.Id)
                return;
            _selectedVoice = value;
            try
            {
                ApplicationData.Current.LocalSettings.Values[VoiceSettingKey] = value.Id;
            }
            catch (Exception)
            {
                // Settings unavailable — the choice just won't persist.
            }
        }
    }

    /// <summary>
    /// Listens once (dictation) and returns the recognized text.
    /// Never throws; failures come back as a friendly error message.
    /// </summary>
    public async Task<(bool Ok, string Text, string Error)> BeginRecognitionAsync()
    {
        SpeechRecognizer? recognizer = null;
        try
        {
            recognizer = new SpeechRecognizer();
            recognizer.Constraints.Add(new SpeechRecognitionTopicConstraint(
                SpeechRecognitionScenario.Dictation, "dictation"));
            var compile = await recognizer.CompileConstraintsAsync();
            if (compile.Status != SpeechRecognitionResultStatus.Success)
            {
                recognizer.Dispose();
                return (false, string.Empty,
                    "I couldn't get the speech engine ready on this PC.");
            }

            var result = await recognizer.RecognizeAsync();
            recognizer.Dispose();
            return result.Status switch
            {
                SpeechRecognitionResultStatus.Success => (true, result.Text, string.Empty),
                SpeechRecognitionResultStatus.NoMatch =>
                    (false, string.Empty, "I didn't quite catch that — want to try once more?"),
                _ => (false, string.Empty, "I couldn't hear anything. Check your microphone and try again."),
            };
        }
        catch (Exception ex)
        {
            recognizer?.Dispose();
            return (false, string.Empty, FriendlyMicError(ex));
        }
    }

    /// <summary>
    /// Speaks text aloud. Shaun's VoiceStudio cloned voice is tried first
    /// (starting VoiceStudio automatically when needed); anything that goes
    /// wrong falls back to the Windows system voice. Never throws.
    /// </summary>
    public async Task SpeakAsync(string text)
    {
        try
        {
            if (await _voiceStudioProvider.TrySpeakAsync(text))
                return;
        }
        catch (Exception)
        {
            // Fall through to the system voice below.
        }
        try
        {
            await VoiceProvider.SpeakAsync(text);
        }
        catch (Exception)
        {
            // Speech output is a nice-to-have; the on-screen text carries
            // the message either way.
        }
    }

    public void StopSpeaking()
    {
        try
        {
            VoiceProvider.Stop();
        }
        catch (Exception)
        {
        }
    }

    private static string FriendlyMicError(Exception ex)
    {
        string message = ex.Message ?? string.Empty;
        if (message.Contains("permission", StringComparison.OrdinalIgnoreCase)
            || message.Contains("access", StringComparison.OrdinalIgnoreCase))
            return "wuse needs microphone permission. Allow it in Windows Settings, then try again.";
        if (message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || message.Contains("no capture", StringComparison.OrdinalIgnoreCase))
            return "I couldn't find a microphone on this PC. Plug one in and try again.";
        return "The microphone isn't cooperating right now. Please try again in a moment.";
    }

    /// <summary>
    /// Default voice: a warm, calm male voice — preferring known friendly
    /// neural voices (Guy/Davis/Tony), then any male voice, then the system
    /// default.
    /// </summary>
    private static VoiceInformation? PickDefaultVoice()
    {
        try
        {
            var voices = SpeechSynthesizer.AllVoices;
            var males = voices.Where(v => v.Gender == VoiceGender.Male).ToList();
            foreach (var hint in PreferredMaleVoiceHints)
            {
                var match = males.FirstOrDefault(v =>
                    v.DisplayName.Contains(hint, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    return match;
            }
            if (males.Count > 0)
                return males[0];
            return new SpeechSynthesizer().Voice;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static VoiceInformation? LoadSavedVoice()
    {
        try
        {
            var saved = ApplicationData.Current.LocalSettings.Values[VoiceSettingKey] as string;
            if (string.IsNullOrEmpty(saved))
                return null;
            return SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.Id == saved);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
