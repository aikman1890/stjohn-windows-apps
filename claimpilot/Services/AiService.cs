using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Text;

namespace ClaimPilot.Services;

/// <summary>
/// Optional on-device AI features (Phi Silica via Windows App SDK). Every call
/// is guarded: if the model is not ready, callers disable their buttons with a
/// tooltip instead of calling in. All exceptions are caught and surfaced as
/// plain status text — the app never crashes because AI is unavailable.
/// </summary>
public static class AiService
{
    /// <summary>True only when the on-device language model reports Ready.</summary>
    public static bool IsAvailable()
    {
        try
        {
            return LanguageModel.GetReadyState() == AIFeatureReadyState.Ready;
        }
        catch
        {
            return false;
        }
    }

    public static string UnavailableTooltip =>
        "On-device AI is not available on this PC. It needs a compatible processor and Windows AI components.";

    /// <summary>Explains legal information in plain English. Never gives legal advice.</summary>
    public static async Task<string> ExplainInPlainEnglishAsync(string topic, string detailText)
    {
        try
        {
            var model = await LanguageModel.CreateAsync();
            var prompt =
                "Explain the following legal information in plain, simple English for a non-lawyer. " +
                "Do not give legal advice. Do not predict whether anyone has a winning case. " +
                "Use cautious language such as 'may apply'.\n\nTopic: " + topic + "\n\n" + detailText;
            var response = await model.GenerateResponseAsync(prompt);
            var text = response?.Text;
            return string.IsNullOrWhiteSpace(text)
                ? "The on-device AI did not return an explanation. Please try again later."
                : text;
        }
        catch (Exception ex)
        {
            return "The on-device AI explanation is not available right now. " +
                   LegalText.GeneralInfoNote + " (Details: " + ex.Message + ")";
        }
    }

    /// <summary>Summarizes a timeline with the on-device text summarizer.</summary>
    public static async Task<string> SummarizeTimelineAsync(string timelineText)
    {
        try
        {
            var model = await LanguageModel.CreateAsync();
            var summarizer = new TextSummarizer(model);
            var result = await summarizer.SummarizeAsync(timelineText);
            var text = result?.Text;
            return string.IsNullOrWhiteSpace(text)
                ? "The on-device AI did not return a summary. Please try again later."
                : text;
        }
        catch (Exception ex)
        {
            return "The on-device AI summary is not available right now. " +
                   LegalText.GeneralInfoNote + " (Details: " + ex.Message + ")";
        }
    }
}
