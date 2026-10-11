using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Streams;

namespace Wuse.ViewModels;

/// <summary>
/// Named ink-note pages. Stroke capture/rendering lives in InkNotesControl;
/// this ViewModel owns the page list, persistence, and status text.
/// </summary>
public partial class InkNotesViewModel : ObservableObject
{
    private readonly Services.InkNoteStore _store = new();

    public ObservableCollection<string> PageNames { get; } = new();

    [ObservableProperty]
    private string? _selectedPageName;

    [ObservableProperty]
    private string _newPageName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>Raised when the selected page changes or needs a reload.</summary>
    public event EventHandler<string?>? SelectedPageChanged;

    partial void OnSelectedPageNameChanged(string? value)
    {
        SelectedPageChanged?.Invoke(this, value);
    }

    [RelayCommand]
    private async Task LoadPagesAsync()
    {
        var current = SelectedPageName;
        var names = await _store.GetPageNamesAsync();
        PageNames.Clear();
        foreach (var n in names)
            PageNames.Add(n);

        if (current != null && PageNames.Contains(current))
        {
            // Selection unchanged: force a reload of the current page.
            SelectedPageChanged?.Invoke(this, current);
        }
        else
        {
            SelectedPageName = PageNames.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task AddPageAsync()
    {
        var name = Services.InkNoteStore.SanitizePageName(NewPageName);
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Type a name for the new ink page first.";
            return;
        }
        if (PageNames.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase)))
        {
            StatusMessage = $"An ink page named \"{name}\" already exists.";
            return;
        }

        PageNames.Add(name);
        NewPageName = string.Empty;
        SelectedPageName = name; // raises SelectedPageChanged -> blank canvas
        StatusMessage = $"Created \"{name}\". Draw, then press Save.";
        await Task.CompletedTask;
    }

    /// <summary>Deletes the selected page file (call after the user confirms).</summary>
    public async Task DeleteSelectedPageAsync()
    {
        var name = SelectedPageName;
        if (name == null)
            return;
        await _store.DeleteAsync(name);
        PageNames.Remove(name);
        SelectedPageName = PageNames.FirstOrDefault();
        StatusMessage = $"Deleted \"{name}\".";
    }

    public async Task SaveStrokesAsync(IBuffer isfData, int strokeCount)
    {
        var name = SelectedPageName;
        if (string.IsNullOrEmpty(name))
        {
            StatusMessage = "Add or select an ink page first.";
            return;
        }
        await _store.SaveAsync(name, isfData);
        var plural = strokeCount == 1 ? "" : "s";
        StatusMessage = $"Saved \"{name}\" ({strokeCount} stroke{plural}).";
    }

    public Task<IBuffer?> LoadStrokesAsync()
    {
        var name = SelectedPageName;
        return string.IsNullOrEmpty(name)
            ? Task.FromResult<IBuffer?>(null)
            : _store.LoadAsync(name);
    }
}
