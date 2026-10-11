using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace ClaimPilot.Models;

/// <summary>A user deadline stored in SQLite, shown sorted by due date.</summary>
public partial class DeadlineItem : ObservableObject
{
    public long Id { get; set; }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private DateTime _dueDate = DateTime.Today;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _category = "Other";

    [ObservableProperty]
    private bool _isDone;

    /// <summary>Tracks which calendar day a reminder toast was already sent.</summary>
    public string NotifiedKey { get; set; } = string.Empty;

    public bool IsOverdue => !IsDone && DueDate.Date < DateTime.Today;

    public Visibility OverdueVisibility => IsOverdue ? Visibility.Visible : Visibility.Collapsed;

    public string DueDateDisplay => DueDate.ToString("MMMM d, yyyy");

    /// <summary>Raises change notifications for computed overdue state.</summary>
    public void RefreshOverdue()
    {
        OnPropertyChanged(nameof(IsOverdue));
        OnPropertyChanged(nameof(OverdueVisibility));
    }
}
