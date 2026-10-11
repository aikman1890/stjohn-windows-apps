using CommunityToolkit.Mvvm.ComponentModel;

namespace DayPilot.Models;

/// <summary>
/// A single reminder with a due date/time.
/// </summary>
public partial class ReminderItem : ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private DateTime _dueAt = DateTime.Now.AddHours(1);

    [ObservableProperty]
    private bool _isDone;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.Now;
}
