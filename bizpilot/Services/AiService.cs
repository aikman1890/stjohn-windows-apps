using System;
using System.Threading.Tasks;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Text;

namespace BizPilot.Services;

public sealed class AiService
{
    private LanguageModel? _model;
    private TextSummarizer? _summarizer;

    public bool IsAvailable { get; private set; }

    public string StatusMessage { get; private set; } = "Checking on-device AI...";

    public async Task InitializeAsync()
    {
        try
        {
            var readyState = LanguageModel.GetReadyState();
            if (readyState != AIFeatureReadyState.Ready)
            {
                IsAvailable = false;
                StatusMessage = $"On-device AI is not ready (state: {readyState}). AI features are disabled.";
                return;
            }

            await LanguageModel.EnsureReadyAsync();
            _model = await LanguageModel.CreateAsync();
            _summarizer = new TextSummarizer(_model);
            IsAvailable = true;
            StatusMessage = "On-device AI is ready.";
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            StatusMessage = $"On-device AI is unavailable: {ex.Message}";
        }
    }

    public async Task<string> DraftClientEmailAsync(string bulletPoints, string tone)
    {
        if (!IsAvailable || _model is null)
        {
            return "On-device AI is not available on this PC, so no draft was generated.";
        }

        try
        {
            var result = await _model.GenerateResponseAsync(
                $"Write a professional client email based on these points. Tone: {tone}. Points:\n{bulletPoints}\n\nEmail:");
            return result.Text;
        }
        catch (Exception ex)
        {
            return $"AI request failed: {ex.Message}";
        }
    }

    public async Task<string> SummarizeMeetingNotesAsync(string text)
    {
        if (!IsAvailable || _summarizer is null)
        {
            return "On-device AI is not available on this PC, so no summary was generated.";
        }

        try
        {
            var summary = await _summarizer.SummarizeAsync(text);
            return summary.Text;
        }
        catch (Exception ex)
        {
            return $"AI request failed: {ex.Message}";
        }
    }
}
