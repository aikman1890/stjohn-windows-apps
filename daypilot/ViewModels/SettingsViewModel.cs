using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DayPilot.Services;

namespace DayPilot.ViewModels;

/// <summary>
/// Settings: on-device AI status (with manual recheck), the global hotkey
/// hint, and version info.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly AiService _ai = AiService.Instance;

    public AiService Ai => _ai;

    public string AiStateText => _ai.IsAvailable ? "On-device AI is ready" : "On-device AI is not available";

    public string AiStateDetail => _ai.StatusMessage;

    public string HotkeyHint => "Press Ctrl+Shift+D from anywhere in Windows to bring DayPilot forward and jump to Notes.";

    public string VersionText => "DayPilot 1.0.0 — your on-device AI daily companion. All AI runs locally; nothing leaves this PC.";

    public SettingsViewModel()
    {
        _ai.PropertyChanged += OnAiPropertyChanged;
    }

    private void OnAiPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AiService.IsAvailable) || e.PropertyName == nameof(AiService.StatusMessage))
        {
            OnPropertyChanged(nameof(AiStateText));
            OnPropertyChanged(nameof(AiStateDetail));
        }
    }

    [RelayCommand]
    private async Task RecheckAiAsync()
    {
        await _ai.RecheckAsync();
    }
}
