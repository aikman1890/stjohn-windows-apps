using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Text;

namespace DayPilot.Services;

/// <summary>
/// Singleton wrapper around the on-device Phi Silica language model
/// (Microsoft.Windows.AI). Every AI call is guarded: if the model is not
/// available on this PC, the service reports it and all AI features stay
/// disabled in the UI instead of throwing.
/// </summary>
public sealed partial class AiService : ObservableObject
{
    private static readonly Lazy<AiService> _instance = new(() => new AiService());

    public static AiService Instance => _instance.Value;

    [ObservableProperty]
    private bool _isAvailable;

    [ObservableProperty]
    private string _statusMessage = "On-device AI has not been checked yet.";

    private LanguageModel? _model;
    private bool _initialized;

    private AiService()
    {
    }

    /// <summary>
    /// Probes for the on-device model. Safe to call repeatedly.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;
        _initialized = true;

        try
        {
            if (LanguageModel.GetReadyState() == AIFeatureReadyState.Ready)
            {
                await CreateModelAsync();
                return;
            }

            try
            {
                await LanguageModel.EnsureReadyAsync();
            }
            catch (Exception ex)
            {
                SetUnavailable("On-device AI (Phi Silica) is not available on this PC. The model could not be prepared: " + ex.Message);
                return;
            }

            if (LanguageModel.GetReadyState() == AIFeatureReadyState.Ready)
                await CreateModelAsync();
            else
                SetUnavailable("On-device AI (Phi Silica) is not available on this PC. The model is not ready.");
        }
        catch (Exception ex)
        {
            SetUnavailable("On-device AI (Phi Silica) is not available on this PC: " + ex.Message);
        }
    }

    /// <summary>
    /// Clears the initialized flag so the next InitializeAsync re-probes.
    /// </summary>
    public async Task RecheckAsync()
    {
        _initialized = false;
        await InitializeAsync();
    }

    public async Task<string> SummarizeAsync(string text)
    {
        if (!IsAvailable || _model == null)
            return StatusMessage;
        try
        {
            var summarizer = new TextSummarizer(_model);
            var result = await summarizer.SummarizeAsync(text);
            return result.Text;
        }
        catch (Exception ex)
        {
            SetUnavailable("On-device AI (Phi Silica) is not available on this PC: " + ex.Message);
            return StatusMessage;
        }
    }

    public async Task<string> RewriteAsync(string text)
    {
        if (!IsAvailable || _model == null)
            return StatusMessage;
        try
        {
            var result = await _model.GenerateResponseAsync(
                "Rewrite the following text to be clearer and more concise, keeping the meaning. Return only the rewritten text:\n\n" + text);
            return result.Text;
        }
        catch (Exception ex)
        {
            SetUnavailable("On-device AI (Phi Silica) is not available on this PC: " + ex.Message);
            return StatusMessage;
        }
    }

    public async Task<string> ExtractActionItemsAsync(string text)
    {
        if (!IsAvailable || _model == null)
            return StatusMessage;
        try
        {
            var result = await _model.GenerateResponseAsync(
                "List the action items in the following notes as short bullet lines starting with '- '. If none, reply 'No action items found.':\n\n" + text);
            return result.Text;
        }
        catch (Exception ex)
        {
            SetUnavailable("On-device AI (Phi Silica) is not available on this PC: " + ex.Message);
            return StatusMessage;
        }
    }

    /// <summary>
    /// Conversational fallback for the voice assistant: a short, warm,
    /// friendly answer (one or two sentences) to anything that isn't a
    /// reminder, note, to-do, day question, or web lookup.
    /// </summary>
    public async Task<string> ChatAsync(string prompt)
    {
        if (!IsAvailable || _model == null)
            return StatusMessage;
        try
        {
            var result = await _model.GenerateResponseAsync(
                "You are DayPilot, a warm and friendly voice assistant. " +
                "Reply in one or two short, cheerful, conversational sentences, " +
                "as if speaking out loud:\n\n" + prompt);
            return result.Text;
        }
        catch (Exception ex)
        {
            SetUnavailable("On-device AI (Phi Silica) is not available on this PC: " + ex.Message);
            return StatusMessage;
        }
    }

    private async Task CreateModelAsync()
    {
        try
        {
            _model = await LanguageModel.CreateAsync();
            IsAvailable = true;
            StatusMessage = "On-device AI (Phi Silica) is ready.";
        }
        catch (Exception ex)
        {
            SetUnavailable("On-device AI (Phi Silica) is not available on this PC: " + ex.Message);
        }
    }

    private void SetUnavailable(string message)
    {
        _model = null;
        IsAvailable = false;
        StatusMessage = message;
    }
}
