using System.Collections.ObjectModel;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClaimPilot.ViewModels;

/// <summary>
/// Deadlines: user deadlines in SQLite, sorted by due date, with overdue
/// highlighting and local toast reminders (see NotificationService).
/// </summary>
public partial class DeadlinesViewModel : ObservableObject
{
    public ObservableCollection<DeadlineItem> Deadlines { get; } = new();

    [ObservableProperty]
    private string _newTitle = string.Empty;

    [ObservableProperty]
    private string _newNotes = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _newDueDate = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private string _newCategory = "Other";

    public ObservableCollection<string> Categories { get; } = new()
    {
        "Court Filing", "Response", "Payment", "Meeting", "Bankruptcy", "Other"
    };

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public string DisclaimerText => LegalText.Disclaimer;

    public string IntroText =>
        "Track every deadline here. ClaimPilot shows a local reminder toast for deadlines due within 24 hours " +
        "or overdue. Deadlines are stored only on this PC.";

    public async Task LoadAsync()
    {
        try
        {
            var items = await DatabaseService.Instance.GetDeadlinesAsync();
            Deadlines.Clear();
            foreach (var item in items.OrderBy(d => d.DueDate))
            {
                item.RefreshOverdue();
                Deadlines.Add(item);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not load deadlines: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task AddDeadlineAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            StatusMessage = "Give the deadline a title first.";
            return;
        }

        try
        {
            var item = new DeadlineItem
            {
                Title = NewTitle.Trim(),
                DueDate = NewDueDate.Date,
                Notes = NewNotes.Trim(),
                Category = NewCategory
            };
            await DatabaseService.Instance.AddDeadlineAsync(item);
            NewTitle = string.Empty;
            NewNotes = string.Empty;
            NewDueDate = DateTimeOffset.Now.Date;
            StatusMessage = "Deadline saved.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not save the deadline: " + ex.Message;
        }
    }

    public async Task DeleteDeadlineAsync(DeadlineItem item)
    {
        if (item is null)
        {
            return;
        }

        try
        {
            await DatabaseService.Instance.DeleteDeadlineAsync(item.Id);
            Deadlines.Remove(item);
            StatusMessage = "Deadline deleted.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not delete the deadline: " + ex.Message;
        }
    }

    public async Task SaveDeadlineAsync(DeadlineItem item)
    {
        if (item is null)
        {
            return;
        }

        try
        {
            item.RefreshOverdue();
            await DatabaseService.Instance.UpdateDeadlineAsync(item);
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not save the deadline: " + ex.Message;
        }
    }

    public void RefreshOverdueStates()
    {
        foreach (var item in Deadlines)
        {
            item.RefreshOverdue();
        }
    }
}
