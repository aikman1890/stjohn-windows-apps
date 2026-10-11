using System.Collections.ObjectModel;
using ClaimPilot.Data;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace ClaimPilot.ViewModels;

/// <summary>Court guide: which court handles what, finding your court, filing, what to bring, etiquette.</summary>
public partial class CourtGuideViewModel : ObservableObject
{
    public ObservableCollection<GuideSection> Sections { get; } =
        new(LegalMattersContent.GetCourtGuideSections());

    public bool IsUnlocked => LegalMattersGate.IsUnlocked();

    public Visibility LockedVisibility => IsUnlocked ? Visibility.Collapsed : Visibility.Visible;

    public Visibility UnlockedVisibility => IsUnlocked ? Visibility.Visible : Visibility.Collapsed;

    public string AddOnBlurb =>
        $"{LegalMattersGate.AddOnName} is a {LegalMattersGate.AddOnPrice} add-on. " +
        "It unlocks traffic tickets, criminal cases, federal matters, court guides, and the case builder.";

    public string DisclaimerText =>
        "ClaimPilot is not a lawyer and does not give legal advice. This is general legal information only.";
}
