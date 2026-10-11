using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;

namespace DayPilot.Services;

/// <summary>
/// Outcome of one listening turn.
/// </summary>
public sealed class ListenOutcome
{
    public bool HeardSomething { get; init; }
    public string Text { get; init; } = string.Empty;
    /// <summary>Friendly message for the user when nothing was heard.</summary>
    public string Message { get; init; } = string.Empty;
    public bool WasCancelled { get; init; }
}

/// <summary>
/// Shared speech engine for DayPilot: microphone speech-to-text
/// (SpeechRecognizer, dictation grammar) and warm, friendly text-to-speech
/// (SpeechSynthesizer via SSML + MediaPlayer).
///
/// Everything is defensive: every speech API call is wrapped in try/catch and
/// surfaces a friendly message instead of throwing, because microphones,
/// speech engines, and installed voices vary wildly across PCs.
/// </summary>
public sealed class SpeechService
{
    private static readonly Lazy<SpeechService> _instance = new(() => new SpeechService());

    public static SpeechService Instance => _instance.Value;

    private const string VoiceSettingKey = "DayPilotVoiceId";
    private const string VoiceStudioProfileKey = "DayPilotVoiceStudioProfile";

    // Warm, friendly, conversational prosody — a cheerful, helpful neighbor,
    // not a bedtime narrator and not a robot auctioneer. Just under normal
    // rate so it feels unhurried; natural medium pitch and volume.
    private const string SsmlRate = "0.95";
    private const string SsmlPitch = "medium";
    private const string SsmlVolume = "medium";

    // Male voice names we prefer, in order (friendly neural voices first).
    private static readonly string[] PreferredMaleVoiceHints =
    {
        "guy", "davis", "tony", "christopher", "david", "mark",
        "eric", "jacob", "brandon", "ryan", "jason",
    };

    private MediaPlayer? _player;
    private VoiceInformation? _selectedVoice;

    // Shaun's cloned voice via VoiceStudio (local). Tried first on every
    // SpeakAsync; the Windows system voice below is only the fallback.
    private readonly VoiceStudioVoiceProvider _voiceStudio;

    private SpeechService()
    {
        _selectedVoice = LoadSavedVoice() ?? PickDefaultVoice();
        _voiceStudio = new VoiceStudioVoiceProvider(() => VoiceStudioProfile);
    }

    /// <summary>
    /// Name of the VoiceStudio voice profile DayPilot speaks through
    /// (Shaun's cloned voice, default "shaun2"). Empty disables the
    /// VoiceStudio provider and uses the Windows voice picker choice.
    /// </summary>
    public string VoiceStudioProfile
    {
        get
        {
            try
            {
                var saved = ApplicationData.Current.LocalSettings.Values[VoiceStudioProfileKey] as string;
                return saved ?? "shaun2";
            }
            catch
            {
                return "shaun2";
            }
        }
        set
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values[VoiceStudioProfileKey] = value ?? string.Empty;
            }
            catch
            {
                // Settings unavailable — the choice just won't persist.
            }
        }
    }

    /// <summary>All voices installed on this PC (for the Settings picker).</summary>
    public IReadOnlyList<VoiceInformation> AvailableVoices
    {
        get
        {
            try { return SpeechSynthesizer.AllVoices; }
            catch (Exception) { return Array.Empty<VoiceInformation>(); }
        }
    }

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
    /// Listens once with the dictation grammar. Stops automatically on a
    /// pause/end of utterance, or when <paramref name="cancellationToken"/>
    /// is cancelled (tap-again-to-stop). Never throws.
    /// </summary>
    public async Task<ListenOutcome> ListenOnceAsync(CancellationToken cancellationToken = default)
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
                return new ListenOutcome
                {
                    Message = "I couldn't get the speech engine ready on this PC.",
                };
            }

            var result = await recognizer.RecognizeAsync().AsTask(cancellationToken);
            return result.Status switch
            {
                SpeechRecognitionResultStatus.Success => new ListenOutcome
                {
                    HeardSomething = true,
                    Text = result.Text,
                },
                SpeechRecognitionResultStatus.NoMatch => new ListenOutcome
                {
                    Message = "I didn't quite catch that — want to try once more?",
                },
                _ => new ListenOutcome
                {
                    Message = "I couldn't hear anything. Check your microphone and try again.",
                },
            };
        }
        catch (OperationCanceledException)
        {
            return new ListenOutcome { WasCancelled = true };
        }
        catch (Exception ex)
        {
            return new ListenOutcome { Message = FriendlyMicError(ex) };
        }
        finally
        {
            recognizer?.Dispose();
        }
    }

    /// <summary>
    /// Speaks text aloud — in Shaun's cloned voice first (VoiceStudio,
    /// profile <see cref="VoiceStudioProfile"/>), falling back to the
    /// selected Windows voice with warm, friendly SSML prosody.
    /// Stops any current speech first. Never throws.
    /// </summary>
    public async Task SpeakAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        try
        {
            // Shaun's voice, always, when available.
            if (await _voiceStudio.TrySpeakAsync(text))
                return;
        }
        catch
        {
            // Fall through to the system voice.
        }
        try
        {
            StopSpeaking();

            using var synthesizer = new SpeechSynthesizer();
            if (_selectedVoice != null)
            {
                try { synthesizer.Voice = _selectedVoice; }
                catch (Exception) { /* fall back to the default voice */ }
            }

            // The voice is set on the synthesizer in code, so the SSML only
            // carries prosody — no <voice> element, avoiding name mismatches.
            string ssml =
                "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\">" +
                "<prosody rate=\"" + SsmlRate + "\" pitch=\"" + SsmlPitch + "\" volume=\"" + SsmlVolume + "\">" +
                EscapeSsml(text) +
                "</prosody></speak>";

            var stream = await synthesizer.SpeakSsmlAsync(ssml);

            // MediaPlayer must be created on the UI thread; callers are page
            // code-behind handlers, which run on the UI thread.
            _player ??= new MediaPlayer();
            _player.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
            _player.Play();
        }
        catch (Exception)
        {
            // Speech output is a nice-to-have; the on-screen text carries
            // the message either way.
        }
    }

    public void StopSpeaking()
    {
        try { _voiceStudio.Stop(); }
        catch
        {
        }
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

    private static string FriendlyMicError(Exception ex)
    {
        string message = ex.Message ?? string.Empty;
        if (message.Contains("permission", StringComparison.OrdinalIgnoreCase)
            || message.Contains("access", StringComparison.OrdinalIgnoreCase))
            return "DayPilot needs microphone permission. Allow it in Windows Settings, then try again.";
        if (message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || message.Contains("no capture", StringComparison.OrdinalIgnoreCase))
            return "I couldn't find a microphone on this PC. Plug one in and try again.";
        return "The microphone isn't cooperating right now. Please try again in a moment.";
    }

    /// <summary>
    /// Default voice: the warmest-sounding installed male voice (preferring
    /// known friendly neural voices like Guy/Davis/Tony), then any male
    /// voice, then the system default.
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
