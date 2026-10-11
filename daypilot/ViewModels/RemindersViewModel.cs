using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DayPilot.Models;
using DayPilot.Services;

namespace DayPilot.ViewModels;

/// <summary>
/// Reminder list with natural-language quick add (parsed locally by
/// ReminderParser) and a live "how DayPilot understood it" preview.
/// </summary>
public partial class RemindersViewModel : ObservableObject
{
    private readonly ReminderStore _store = new();
    private readonly ReminderParser _parser = new();

    public ObservableCollection<ReminderItem> Reminders { get; } = new();

    [ObservableProperty]
    private string _nlInput = string.Empty;

    [ObservableProperty]
    private string _parsePreview = "Type a reminder above to see how DayPilot understands it.";

    [RelayCommand]
    private async Task LoadAsync()
    {
        Reminders.Clear();
        foreach (var reminder in (await _store.GetAllAsync()).OrderBy(r => r.DueAt))
            Reminders.Add(reminder);
    }

    partial void OnNlInputChanged(string value)
    {
        AddReminderCommand.NotifyCanExecuteChanged();
        if (string.IsNullOrWhiteSpace(value))
        {
            ParsePreview = "Type a reminder above to see how DayPilot understands it.";
            return;
        }
        var (due, text) = _parser.Parse(value);
        ParsePreview = $"Understood as: \"{text}\" — due {due:dddd, MMMM d} at {due:h:mm tt}.";
    }

    private bool CanAddReminder() => !string.IsNullOrWhiteSpace(NlInput);

    [RelayCommand(CanExecute = nameof(CanAddReminder))]
    private async Task AddReminderAsync()
    {
        var (due, text) = _parser.Parse(NlInput);
        var item = new ReminderItem { Text = text, DueAt = due };
        await _store.AddAsync(item);

        int index = Reminders.TakeWhile(r => r.DueAt <= due).Count();
        Reminders.Insert(index, item);

        NlInput = string.Empty;
        NotificationService.Instance.ShowToast("Reminder set", $"{text} — {due:g}");
    }

    [RelayCommand]
    private async Task ToggleDoneAsync(ReminderItem? item)
    {
        if (item == null)
            return;
        await _store.UpdateAsync(item);
    }

    [RelayCommand]
    private async Task DeleteReminderAsync(ReminderItem? item)
    {
        if (item == null)
            return;
        await _store.DeleteAsync(item.Id);
        Reminders.Remove(item);
    }
}
