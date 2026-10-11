using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClaimPilot.Models;

/// <summary>
/// The case file assembled by the Case Builder: name, type, parties, timeline,
/// claims/issues, and an evidence checklist. Deadlines entered in the builder
/// are also saved to the deadline tracker (see DeadlinesViewModel).
/// </summary>
public partial class CaseFile : ObservableObject
{
    [ObservableProperty]
    private string _caseName = string.Empty;

    [ObservableProperty]
    private string _caseType = "Civil";

    public ObservableCollection<string> Parties { get; } = new();

    public ObservableCollection<TimelineEvent> Timeline { get; } = new();

    public ObservableCollection<string> Claims { get; } = new();

    public ObservableCollection<ChecklistItem> EvidenceChecklist { get; } = new();

    public int TimelineCount => Timeline.Count;

    public int PartiesCount => Parties.Count;

    public void NotifyCountsChanged()
    {
        OnPropertyChanged(nameof(TimelineCount));
        OnPropertyChanged(nameof(PartiesCount));
    }
}
