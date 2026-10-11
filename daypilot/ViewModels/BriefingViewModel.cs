using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DayPilot.Models;
using DayPilot.Services;

namespace DayPilot.ViewModels;

/// <summary>
/// Daily briefing: counts plus the next upcoming reminders.
/// </summary>
public partial class BriefingViewModel : ObservableObject
{
    private readonly NoteStore _noteStore = new();
    private readonly ReminderStore _reminderStore = new();

    public string TodayDate => DateTime.Now.ToString("dddd, MMMM d, yyyy");

    [ObservableProperty]
    private int _noteCount;

    [ObservableProperty]
    private int _dueTodayCount;

    [ObservableProperty]
    private int _overdueCount;

    public ObservableCollection<ReminderItem> UpcomingReminders { get; } = new();

    [RelayCommand]
    private async Task LoadAsync()
    {
        var notes = await _noteStore.GetAllAsync();
        NoteCount = notes.Count;

        var now = DateTime.Now;
        var today = now.Date;
        var reminders = await _reminderStore.GetAllAsync();

        DueTodayCount = reminders.Count(r => !r.IsDone && r.DueAt.Date == today && r.DueAt >= now);
        OverdueCount = reminders.Count(r => !r.IsDone && r.DueAt < now);

        UpcomingReminders.Clear();
        foreach (var reminder in reminders.Where(r => !r.IsDone).OrderBy(r => r.DueAt).Take(5))
            UpcomingReminders.Add(reminder);

        OnPropertyChanged(nameof(TodayDate));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }
}
