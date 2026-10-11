using System.Collections.ObjectModel;
using ClaimPilot.Data;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace ClaimPilot.ViewModels;

/// <summary>
/// Civil Rights page: the six claim guides, the three honest notes, and the
/// optional on-device AI plain-English explainer (disabled when unavailable).
/// </summary>
public partial class ClaimGuidesViewModel : ObservableObject
{
    public ObservableCollection<ClaimGuide> Guides { get; } = new(ClaimGuides.GetGuides());

    public ObservableCollection<HonestNote> HonestNotes { get; } = new(ClaimGuides.GetHonestNotes());

    [ObservableProperty]
    private string _explanationTitle = string.Empty;

    [ObservableProperty]
    private string _explanationText = string.Empty;

    [ObservableProperty]
    private Visibility _explanationVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private bool _isAiWorking;

    public bool IsAiAvailable => AiService.IsAvailable();

    public string AiButtonTooltip => IsAiAvailable
        ? "Use the on-device AI to explain this guide in plain English."
        : AiService.UnavailableTooltip;

    public string IntroText =>
        "General information about six common federal civil-rights claim types under 42 U.S.C. 1983. " +
        "Read the honest notes first. Nothing here is legal advice, and nothing here predicts any outcome.";

    public string DisclaimerText => LegalText.Disclaimer + " " + LegalText.NeverFilesNote;

    /// <summary>Explains one guide in plain English using on-device AI.</summary>
    public async Task ExplainGuideAsync(ClaimGuide guide)
    {
        if (guide is null || IsAiWorking)
        {
            return;
        }

        if (!IsAiAvailable)
        {
            return;
        }

        IsAiWorking = true;
        ExplanationTitle = "Plain-English explanation: " + guide.Title;
        ExplanationText = "Working on-device…";
        ExplanationVisibility = Visibility.Visible;

        try
        {
            var detail = "What it is: " + guide.WhatItIs +
                         "\n\nWhat a plaintiff must generally show: " + guide.WhatPlaintiffMustShow +
                         "\n\nCommon defenses: " + guide.CommonDefenses;
            ExplanationText = await AiService.ExplainInPlainEnglishAsync(guide.Title, detail);
        }
        finally
        {
            IsAiWorking = false;
        }
    }
}
