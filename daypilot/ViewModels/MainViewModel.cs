using CommunityToolkit.Mvvm.ComponentModel;
using DayPilot.Services;
using DayPilot.Views;

namespace DayPilot.ViewModels;

/// <summary>
/// Shell view-model: owns page selection/navigation requests and exposes
/// the shared AiService singleton to the shell.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly Dictionary<string, Type> _pages = new()
    {
        ["briefing"] = typeof(BriefingPage),
        ["notes"] = typeof(NotesPage),
        ["reminders"] = typeof(RemindersPage),
        ["tasks"] = typeof(TasksPage),
        ["settings"] = typeof(SettingsPage),
    };

    public AiService Ai => AiService.Instance;

    [ObservableProperty]
    private string _selectedPageTag = string.Empty;

    public event EventHandler<Type>? NavigateRequested;

    public void SelectPage(string tag)
    {
        if (_pages.TryGetValue(tag, out var pageType))
        {
            SelectedPageTag = tag;
            NavigateRequested?.Invoke(this, pageType);
        }
    }
}
