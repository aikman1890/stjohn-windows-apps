using CommunityToolkit.Mvvm.ComponentModel;

namespace DayPilot.Models;

/// <summary>
/// A single user note.
/// </summary>
public partial class Note : ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _body = string.Empty;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.Now;

    [ObservableProperty]
    private DateTime _updatedAt = DateTime.Now;

    [ObservableProperty]
    private string _tags = string.Empty;
}
