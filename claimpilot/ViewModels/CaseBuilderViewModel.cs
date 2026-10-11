using System.Collections.ObjectModel;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace ClaimPilot.ViewModels;

/// <summary>
/// Case Builder: a guided builder for any legal matter. Assembles a case file
/// (name, type, parties, timeline, claims/issues, evidence checklist) in memory.
/// Deadline entries are also saved to the deadline tracker.
/// </summary>
public partial class CaseBuilderViewModel : ObservableObject
{
    public CaseFile CaseFile { get; } = new();

    public ObservableCollection<string> CaseTypes { get; } = new()
    {
        "Traffic", "Criminal", "Civil", "Federal", "Other"
    };

    [ObservableProperty]
    private string _newParty = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _newEventDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string _newEventTitle = string.Empty;

    [ObservableProperty]
    private string _newEventDetails = string.Empty;

    [ObservableProperty]
    private string _newClaim = string.Empty;

    [ObservableProperty]
    private string _newEvidenceText = string.Empty;

    [ObservableProperty]
    private string _newDeadlineTitle = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _newDeadlineDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public bool IsUnlocked => LegalMattersGate.IsUnlocked();

    public Visibility LockedVisibility => IsUnlocked ? Visibility.Collapsed : Visibility.Visible;

    public Visibility UnlockedVisibility => IsUnlocked ? Visibility.Visible : Visibility.Collapsed;

    public string AddOnBlurb =>
        $"{LegalMattersGate.AddOnName} is a {LegalMattersGate.AddOnPrice} add-on. " +
        "It unlocks traffic tickets, criminal cases, federal matters, court guides, and the case builder.";

    public string DisclaimerText =>
        "ClaimPilot is not a lawyer and does not give legal advice. This is general legal information only.";

    public string IntroText =>
        "Build a case file for any legal matter: name the case, add the parties, lay out a timeline of " +
        "events, list your claims or issues, and check off evidence as you gather it. Deadlines you add " +
        "here also go into the deadline tracker with reminders.";

    [RelayCommand]
    private void AddParty()
    {
        if (string.IsNullOrWhiteSpace(NewParty))
        {
            StatusMessage = "Type a party name first (a person, company, or agency).";
            return;
        }

        CaseFile.Parties.Add(NewParty.Trim());
        NewParty = string.Empty;
        CaseFile.NotifyCountsChanged();
        StatusMessage = string.Empty;
    }

    public void RemoveParty(string party)
    {
        CaseFile.Parties.Remove(party);
        CaseFile.NotifyCountsChanged();
    }

    [RelayCommand]
    private void AddTimelineEvent()
    {
        if (string.IsNullOrWhiteSpace(NewEventTitle))
        {
            StatusMessage = "Give the event a title first.";
            return;
        }

        CaseFile.Timeline.Add(new TimelineEvent
        {
            EventDate = NewEventDate.Date,
            Title = NewEventTitle.Trim(),
            Details = NewEventDetails.Trim()
        });
        NewEventTitle = string.Empty;
        NewEventDetails = string.Empty;
        CaseFile.NotifyCountsChanged();
        StatusMessage = string.Empty;
    }

    public void RemoveTimelineEvent(TimelineEvent item)
    {
        CaseFile.Timeline.Remove(item);
        CaseFile.NotifyCountsChanged();
    }

    [RelayCommand]
    private void AddClaim()
    {
        if (string.IsNullOrWhiteSpace(NewClaim))
        {
            StatusMessage = "Type a claim or issue first.";
            return;
        }

        CaseFile.Claims.Add(NewClaim.Trim());
        NewClaim = string.Empty;
        StatusMessage = string.Empty;
    }

    public void RemoveClaim(string claim)
    {
        CaseFile.Claims.Remove(claim);
    }

    [RelayCommand]
    private void AddEvidence()
    {
        if (string.IsNullOrWhiteSpace(NewEvidenceText))
        {
            StatusMessage = "Type an evidence item first (a document, photo, or record to gather).";
            return;
        }

        CaseFile.EvidenceChecklist.Add(new ChecklistItem(NewEvidenceText.Trim()));
        NewEvidenceText = string.Empty;
        StatusMessage = string.Empty;
    }

    public void RemoveEvidence(ChecklistItem item)
    {
        CaseFile.EvidenceChecklist.Remove(item);
    }

    [RelayCommand]
    private async Task AddDeadlineAsync()
    {
        if (string.IsNullOrWhiteSpace(NewDeadlineTitle))
        {
            StatusMessage = "Give the deadline a title first.";
            return;
        }

        try
        {
            var label = string.IsNullOrWhiteSpace(CaseFile.CaseName)
                ? NewDeadlineTitle.Trim()
                : CaseFile.CaseName.Trim() + " — " + NewDeadlineTitle.Trim();

            await DatabaseService.Instance.AddDeadlineAsync(new DeadlineItem
            {
                Title = label,
                DueDate = NewDeadlineDate.Date,
                Notes = "Added from the Case Builder.",
                Category = "Case Builder"
            });

            NewDeadlineTitle = string.Empty;
            StatusMessage = "Deadline saved to the deadline tracker.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not save the deadline: " + ex.Message;
        }
    }

    [RelayCommand]
    private void ClearCase()
    {
        CaseFile.CaseName = string.Empty;
        CaseFile.Parties.Clear();
        CaseFile.Timeline.Clear();
        CaseFile.Claims.Clear();
        CaseFile.EvidenceChecklist.Clear();
        CaseFile.NotifyCountsChanged();
        StatusMessage = "Case file cleared. Deadlines already saved to the tracker were kept.";
    }
}
