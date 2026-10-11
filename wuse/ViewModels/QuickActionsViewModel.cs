using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wuse.Services;

namespace Wuse.ViewModels;

public partial class QuickActionsViewModel : ObservableObject
{
    private readonly QuickActionService _quick;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string folderPath = string.Empty;

    [ObservableProperty]
    private string resultMessage = string.Empty;

    [ObservableProperty]
    private string machineInfo = string.Empty;

    public QuickActionsViewModel(QuickActionService quick)
    {
        _quick = quick;
    }

    public async Task InitializeAsync()
    {
        MachineInfo = await _quick.MachineInfoAsync();
    }

    [RelayCommand]
    private async Task OpenCalculatorAsync() => ResultMessage = await _quick.OpenAppAsync("calculator");

    [RelayCommand]
    private async Task OpenNotepadAsync() => ResultMessage = await _quick.OpenAppAsync("notepad");

    [RelayCommand]
    private async Task OpenPaintAsync() => ResultMessage = await _quick.OpenAppAsync("paint");

    [RelayCommand]
    private async Task OpenSettingsAsync() => ResultMessage = await _quick.OpenAppAsync("settings");

    [RelayCommand]
    private async Task OpenStoreAsync() => ResultMessage = await _quick.OpenAppAsync("store");

    [RelayCommand]
    private async Task OpenExplorerAsync() => ResultMessage = await _quick.OpenAppAsync("explorer");

    [RelayCommand]
    private async Task OpenTerminalAsync() => ResultMessage = await _quick.OpenAppAsync("terminal");

    [RelayCommand]
    private async Task OpenBrowserAsync() => ResultMessage = await _quick.OpenAppAsync("browser");

    [RelayCommand]
    private async Task WebSearchAsync() => ResultMessage = await _quick.WebSearchAsync(SearchQuery);

    [RelayCommand]
    private async Task OpenFolderAsync() => ResultMessage = await _quick.OpenFolderAsync(FolderPath);

    [RelayCommand]
    private async Task LoadMachineInfoAsync() => MachineInfo = await _quick.MachineInfoAsync();

    [RelayCommand]
    private async Task CopyMachineInfoAsync() => ResultMessage = await _quick.CopyMachineInfoAsync();
}
