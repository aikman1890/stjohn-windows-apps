using RangePilot.Models;
using RangePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RangePilot.Views;

public sealed partial class AssetsPage : Page
{
    private readonly JsonListStore<AssetItem> _store = new("assets.json");
    private readonly JsonListStore<AssetLogEntry> _logStore = new("assetlog.json");
    private List<AssetItem> _assets = new();
    private AssetItem? _selected;

    public AssetsPage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = new[] { "Vehicle", "Range lane", "Equipment", "Other" };
        StatusBox.ItemsSource = new[] { "Up", "Down", "Maintenance" };
        LogKindBox.ItemsSource = new[] { "Maintenance", "Usage", "Note" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _assets = (await _store.LoadAsync()).OrderBy(a => a.Name).ToList();
        AssetList.ItemsSource = _assets;
        if (_assets.Count > 0) AssetList.SelectedIndex = 0;
    }

    private async void AssetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = AssetList.SelectedItem as AssetItem;
        if (_selected is null) { LogList.ItemsSource = null; return; }
        NameBox.Text = _selected.Name;
        KindBox.SelectedItem = _selected.Kind;
        StatusBox.SelectedItem = _selected.Status;
        NextBox.Text = _selected.NextServiceNote;
        var log = await _logStore.LoadAsync();
        LogList.ItemsSource = log.Where(l => l.AssetId == _selected.Id)
            .OrderByDescending(l => l.Date).ToList();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "Name the asset."; return; }
        var a = _selected ?? new AssetItem();
        a.Name = NameBox.Text.Trim();
        a.Kind = KindBox.SelectedItem as string ?? a.Kind;
        a.Status = StatusBox.SelectedItem as string ?? a.Status;
        a.NextServiceNote = NextBox.Text.Trim();
        if (_selected is null) _assets.Add(a);
        await _store.SaveAsync(_assets);
        _assets = _assets.OrderBy(x => x.Name).ToList();
        AssetList.ItemsSource = _assets;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        AssetList.SelectedItem = null;
        NameBox.Text = NextBox.Text = string.Empty;
        StatusBox.SelectedItem = "Up";
        LogList.ItemsSource = null;
    }

    private async void LogButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the asset first."; return; }
        if (string.IsNullOrWhiteSpace(LogDetailBox.Text)) { ErrorText.Text = "Write the entry."; return; }
        var log = await _logStore.LoadAsync();
        log.Add(new AssetLogEntry
        {
            AssetId = _selected.Id, AssetLabel = _selected.Name, Date = DateTimeOffset.Now,
            Kind = LogKindBox.SelectedItem as string ?? "Note", Detail = LogDetailBox.Text.Trim(),
        });
        await _logStore.SaveAsync(log);
        LogDetailBox.Text = string.Empty;
        AssetList_SelectionChanged(this, null!);
    }
}
