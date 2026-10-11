using System.Collections.ObjectModel;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace ClaimPilot.ViewModels;

/// <summary>
/// Evidence Vault: attached files (copied to LocalFolder/Vault), a date-sorted
/// timeline, a civil-rights incident template, AI timeline summaries (guarded),
/// and a one-click QuestPDF evidence packet export.
/// </summary>
public partial class EvidenceVaultViewModel : ObservableObject
{
    public ObservableCollection<EvidenceItem> EvidenceItems { get; } = new();
    public ObservableCollection<TimelineEvent> TimelineEvents { get; } = new();

    // New evidence form.
    [ObservableProperty]
    private string _newEvidenceTitle = string.Empty;

    [ObservableProperty]
    private string _newEvidenceCategory = "Photo";

    [ObservableProperty]
    private string _newEvidenceDescription = string.Empty;

    public ObservableCollection<string> EvidenceCategories { get; } = new()
    {
        "Photo", "Video", "Document", "Medical Record", "Correspondence", "Police Report", "Other"
    };

    // New timeline-event form.
    [ObservableProperty]
    private string _newEventTitle = string.Empty;

    [ObservableProperty]
    private string _newEventDetails = string.Empty;

    [ObservableProperty]
    private string _newEventCategory = "Incident";

    [ObservableProperty]
    private DateTimeOffset _newEventDate = DateTimeOffset.Now.Date;

    // Civil-rights incident template fields.
    [ObservableProperty]
    private DateTimeOffset _incidentDate = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private string _incidentLocation = string.Empty;

    [ObservableProperty]
    private string _incidentOfficers = string.Empty;

    [ObservableProperty]
    private string _incidentWitnesses = string.Empty;

    [ObservableProperty]
    private string _incidentComplaintNumbers = string.Empty;

    public ObservableCollection<ChecklistItem> IncidentChecklist { get; } = new()
    {
        new ChecklistItem("Date, time, and exact location recorded"),
        new ChecklistItem("Officers involved identified, with badge numbers where known"),
        new ChecklistItem("Witness names and contact information recorded"),
        new ChecklistItem("Photos / video saved to the vault"),
        new ChecklistItem("Medical records or photos of injuries saved"),
        new ChecklistItem("Complaint or report numbers recorded (internal affairs, civilian review, police report)")
    };

    // AI summary.
    [ObservableProperty]
    private string _timelineSummary = string.Empty;

    [ObservableProperty]
    private Visibility _summaryVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private bool _isAiWorking;

    public bool IsAiAvailable => AiService.IsAvailable();

    public string AiButtonTooltip => IsAiAvailable
        ? "Use the on-device AI to summarize your timeline."
        : AiService.UnavailableTooltip;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public string DisclaimerText => LegalText.Disclaimer;

    public string IntroText =>
        "Everything here stays on this PC. Attach files, keep a date-sorted timeline, and export a single " +
        "organized packet when you need one. This is general information, not legal advice.";

    public async Task LoadAsync()
    {
        try
        {
            var evidence = await DatabaseService.Instance.GetEvidenceItemsAsync();
            EvidenceItems.Clear();
            foreach (var item in evidence)
            {
                EvidenceItems.Add(item);
            }

            var events = await DatabaseService.Instance.GetTimelineEventsAsync();
            TimelineEvents.Clear();
            foreach (var e in events.OrderBy(x => x.EventDate))
            {
                TimelineEvents.Add(e);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not load the vault: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task AddEvidenceAsync()
    {
        if (string.IsNullOrWhiteSpace(NewEvidenceTitle))
        {
            StatusMessage = "Give the evidence a title first.";
            return;
        }

        var file = await FilePickerService.PickSingleFileAsync();
        if (file is null)
        {
            StatusMessage = "Attach canceled — no file was saved.";
            return;
        }

        try
        {
            var vaultFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                "Vault", CreationCollisionOption.OpenIfExists);
            var uniqueName = $"{Guid.NewGuid():N}_{file.Name}";
            await file.CopyAsync(vaultFolder, uniqueName, NameCollisionOption.FailIfExists);

            var item = new EvidenceItem
            {
                Title = NewEvidenceTitle.Trim(),
                Category = NewEvidenceCategory,
                ItemDate = DateTime.Today,
                Description = NewEvidenceDescription.Trim(),
                FilePath = Path.Combine(vaultFolder.Path, uniqueName)
            };
            await DatabaseService.Instance.AddEvidenceItemAsync(item);

            NewEvidenceTitle = string.Empty;
            NewEvidenceDescription = string.Empty;
            StatusMessage = $"Saved “{item.Title}” to the vault.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not save the evidence: " + ex.Message;
        }
    }

    public async Task DeleteEvidenceAsync(EvidenceItem item)
    {
        if (item is null)
        {
            return;
        }

        try
        {
            await DatabaseService.Instance.DeleteEvidenceItemAsync(item.Id);
            EvidenceItems.Remove(item);
            StatusMessage = "Evidence record deleted. (The copied file remains in the Vault folder; delete it in File Explorer if you want it gone.)";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not delete the evidence: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task AddTimelineEventAsync()
    {
        if (string.IsNullOrWhiteSpace(NewEventTitle))
        {
            StatusMessage = "Give the timeline event a title first.";
            return;
        }

        try
        {
            var e = new TimelineEvent
            {
                EventDate = NewEventDate.Date,
                Title = NewEventTitle.Trim(),
                Details = NewEventDetails.Trim(),
                Category = NewEventCategory
            };
            await DatabaseService.Instance.AddTimelineEventAsync(e);
            NewEventTitle = string.Empty;
            NewEventDetails = string.Empty;
            StatusMessage = "Timeline event added.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not add the timeline event: " + ex.Message;
        }
    }

    public async Task DeleteTimelineEventAsync(TimelineEvent item)
    {
        if (item is null)
        {
            return;
        }

        try
        {
            await DatabaseService.Instance.DeleteTimelineEventAsync(item.Id);
            TimelineEvents.Remove(item);
            StatusMessage = "Timeline event deleted.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not delete the timeline event: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task AddIncidentToTimelineAsync()
    {
        try
        {
            var checkedItems = IncidentChecklist.Where(c => c.IsChecked).Select(c => c.Text).ToList();
            var details = new System.Text.StringBuilder();
            if (!string.IsNullOrWhiteSpace(IncidentLocation))
            {
                details.AppendLine("Location: " + IncidentLocation.Trim());
            }
            if (!string.IsNullOrWhiteSpace(IncidentOfficers))
            {
                details.AppendLine("Officers involved: " + IncidentOfficers.Trim());
            }
            if (!string.IsNullOrWhiteSpace(IncidentWitnesses))
            {
                details.AppendLine("Witnesses: " + IncidentWitnesses.Trim());
            }
            if (!string.IsNullOrWhiteSpace(IncidentComplaintNumbers))
            {
                details.AppendLine("Complaint/report numbers: " + IncidentComplaintNumbers.Trim());
            }
            if (checkedItems.Count > 0)
            {
                details.AppendLine("Checklist completed: " + string.Join("; ", checkedItems));
            }

            var e = new TimelineEvent
            {
                EventDate = IncidentDate.Date,
                Title = "Civil-rights incident" + (string.IsNullOrWhiteSpace(IncidentLocation) ? string.Empty : " — " + IncidentLocation.Trim()),
                Details = details.ToString().Trim(),
                Category = "Incident"
            };
            await DatabaseService.Instance.AddTimelineEventAsync(e);
            StatusMessage = "Incident added to the timeline.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not add the incident: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task ExportPacketAsync()
    {
        var file = await FilePickerService.PickSaveFileAsync("EvidencePacket", ".pdf", "PDF file");
        if (file is null)
        {
            StatusMessage = "Export canceled.";
            return;
        }

        try
        {
            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Date: {IncidentDate:MMMM d, yyyy}");
            if (!string.IsNullOrWhiteSpace(IncidentLocation))
            {
                summary.AppendLine("Location: " + IncidentLocation.Trim());
            }
            if (!string.IsNullOrWhiteSpace(IncidentOfficers))
            {
                summary.AppendLine("Officers involved: " + IncidentOfficers.Trim());
            }
            if (!string.IsNullOrWhiteSpace(IncidentWitnesses))
            {
                summary.AppendLine("Witnesses: " + IncidentWitnesses.Trim());
            }
            if (!string.IsNullOrWhiteSpace(IncidentComplaintNumbers))
            {
                summary.AppendLine("Complaint/report numbers: " + IncidentComplaintNumbers.Trim());
            }

            PdfService.GenerateEvidencePacketPdf(
                file.Path,
                summary.ToString().Trim(),
                TimelineEvents.OrderBy(e => e.EventDate).ToList(),
                EvidenceItems.ToList());
            StatusMessage = $"Evidence packet exported to {file.Name}.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not export the packet: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task SummarizeTimelineAsync()
    {
        if (IsAiWorking || !IsAiAvailable)
        {
            return;
        }

        if (TimelineEvents.Count == 0)
        {
            StatusMessage = "Add timeline events first.";
            return;
        }

        IsAiWorking = true;
        SummaryVisibility = Visibility.Visible;
        TimelineSummary = "Summarizing on-device…";

        try
        {
            var text = new System.Text.StringBuilder();
            foreach (var e in TimelineEvents.OrderBy(x => x.EventDate))
            {
                text.AppendLine($"{e.EventDateDisplay}: {e.Title}");
                if (!string.IsNullOrWhiteSpace(e.Details))
                {
                    text.AppendLine(e.Details);
                }
                text.AppendLine();
            }
            TimelineSummary = await AiService.SummarizeTimelineAsync(text.ToString());
        }
        finally
        {
            IsAiWorking = false;
        }
    }
}
