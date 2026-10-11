using System.Text;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Text;
using Wuse.Models;

namespace Wuse.Services;

/// <summary>
/// On-device language model access (Windows AI Foundry / Phi Silica).
/// Probes once; every call is wrapped in try/catch and the app falls back
/// to local rule-based replies when the model is unavailable.
/// </summary>
public sealed class AiService
{
    private LanguageModel? _model;
    private bool _probed;

    public bool IsAvailable { get; private set; }

    public string StatusMessage { get; private set; } = "Checking on-device AI…";

    /// <summary>
    /// Probes the on-device model. Safe to call repeatedly; only the first call does work.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_probed) return;
        _probed = true;

        try
        {
            if (LanguageModel.GetReadyState() != AIFeatureReadyState.Ready)
            {
                try
                {
                    await LanguageModel.EnsureReadyAsync();
                }
                catch
                {
                    // Fall through to the readiness check below.
                }
            }

            if (LanguageModel.GetReadyState() == AIFeatureReadyState.Ready)
            {
                _model = await LanguageModel.CreateAsync();
                IsAvailable = true;
                StatusMessage = "On-device AI is ready.";
            }
            else
            {
                IsAvailable = false;
                StatusMessage = "On-device AI isn't available on this PC. wuse still works in local mode — try \"help\".";
            }
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            _model = null;
            StatusMessage = $"On-device AI isn't available on this PC ({ex.Message}). wuse still works in local mode — try \"help\".";
        }
    }

    /// <summary>
    /// Generates a reply. Returns null when the model is unavailable or the call fails,
    /// so callers can fall back to local replies.
    /// </summary>
    public async Task<string?> ChatAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, string userInput)
    {
        if (!IsAvailable || _model is null) return null;

        try
        {
            var prompt = BuildPrompt(systemPrompt, history, userInput);
            var result = await _model.GenerateResponseAsync(prompt);
            var text = result?.Text?.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }
        catch
        {
            return null;
        }
    }

    private static string BuildPrompt(string systemPrompt, IReadOnlyList<ChatMessage> history, string userInput)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine(systemPrompt);
        prompt.AppendLine();
        foreach (var message in history.TakeLast(12))
        {
            prompt.Append(message.Role == "user" ? "User: " : "Assistant: ");
            prompt.AppendLine(message.Text);
        }
        prompt.Append("User: ");
        prompt.AppendLine(userInput);
        prompt.Append("Assistant:");
        return prompt.ToString();
    }
}
