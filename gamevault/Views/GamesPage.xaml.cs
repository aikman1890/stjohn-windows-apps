using System.Diagnostics;
using System.Text.Json;
using GameVault.Models;
using GameVault.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GameVault.Views;

public sealed partial class GamesPage : Page
{
    private readonly JsonListStore<GameItem> _store = new("games.json");
    private List<GameItem> _items = new();
    private GameItem? _selected;
    private string _folderPath = string.Empty;

    public GamesPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        _folderPath = await LoadFolderPathAsync();
        FolderText.Text = _folderPath.Length == 0 ? "No folder chosen yet." : $"Games folder: {_folderPath}";
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(g => g.Title).ToList();
    }

    private async Task<string> LoadFolderPathAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync("vault-settings.json");
            var json = await FileIO.ReadTextAsync(file);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("FolderPath", out var v) ? v.GetString() ?? string.Empty : string.Empty;
        }
        catch (FileNotFoundException)
        {
            return string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private async Task SaveFolderPathAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync("vault-settings.json", CreationCollisionOption.ReplaceExisting);
        var json = JsonSerializer.Serialize(new Dictionary<string, string> { ["FolderPath"] = _folderPath });
        await FileIO.WriteTextAsync(file, json);
    }

    private async void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.WindowInstance is null) return;
        var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
        picker.FileTypeFilter.Add("*");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var folder = await picker.PickSingleFolderAsync();
        if (folder is null) return;
        _folderPath = folder.Path;
        FolderText.Text = $"Games folder: {_folderPath}";
        await SaveFolderPathAsync();
        await ScanAsync();
    }

    private async void RescanButton_Click(object sender, RoutedEventArgs e)
    {
        if (_folderPath.Length == 0)
        {
            StatusText.Text = "Choose a games folder first.";
            return;
        }
        await ScanAsync();
    }

    private async Task ScanAsync()
    {
        var found = new List<string>();
        try
        {
            var opts = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };
            foreach (var f in Directory.EnumerateFiles(_folderPath, "*", opts))
            {
                var ext = Path.GetExtension(f).ToLowerInvariant();
                if (ext is ".zip" or ".exe" or ".html" or ".htm") found.Add(f);
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Scan failed: {ex.Message}";
            return;
        }

        int added = 0;
        foreach (var f in found)
        {
            if (_items.Any(g => string.Equals(g.FilePath, f, StringComparison.OrdinalIgnoreCase))) continue;
            var ext = Path.GetExtension(f).ToLowerInvariant();
            _items.Add(new GameItem
            {
                Title = Prettify(Path.GetFileNameWithoutExtension(f)),
                FilePath = f,
                FileKind = ext == ".zip" ? "ZIP" : ext == ".exe" ? "EXE" : "HTML",
            });
            added++;
        }
        await _store.SaveAsync(_items);
        RefreshList();
        StatusText.Text = $"Scan complete: {found.Count} build file(s) found, {added} new added to the catalog.";
    }

    private static string Prettify(string name) =>
        name.Replace('-', ' ').Replace('_', ' ').Trim();

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as GameItem;
        if (_selected is null) return;
        TitleBox.Text = _selected.Title;
        GenreBox.Text = _selected.Genre;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.Title = TitleBox.Text.Trim();
        _selected.Genre = GenreBox.Text.Trim();
        _selected.Notes = NotesBox.Text.Trim();
        await _store.SaveAsync(_items);
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(g => g.Id == _selected.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(g => g.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        TitleBox.Text = string.Empty;
        GenreBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        RefreshList();
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            StatusText.Text = "Select a game first.";
            return;
        }
        try
        {
            if (_selected.FileKind == "ZIP")
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{_selected.FilePath}\"",
                    UseShellExecute = true,
                });
                StatusText.Text = "ZIP build opened in Explorer - extract it, then run the game inside.";
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _selected.FilePath,
                    UseShellExecute = true,
                });
                StatusText.Text = $"Launched {_selected.Title}.";
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Launch failed: {ex.Message}";
        }
    }

    private void ShowButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{_selected.FilePath}\"",
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not open Explorer: {ex.Message}";
        }
    }
}
