using CommunityToolkit.Mvvm.ComponentModel;

namespace DayPilot.Models;

/// <summary>
/// A single to-do task with a completion flag.
/// </summary>
public partial class TodoItem : ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isDone;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.Now;

    [ObservableProperty]
    private DateTime? _completedAt;
}
