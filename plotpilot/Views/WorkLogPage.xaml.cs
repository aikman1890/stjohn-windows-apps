using PlotPilot.Models;
using PlotPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlotPilot.Views;

public sealed partial class WorkLogPage : Page
{
    private readonly JsonListStore<WorkLogEntry> _store = new("worklog.json");
    private List<WorkLogEntry> _items = new();
    private WorkLogEntry? _selected;

    public WorkLogPage()
    {
        this.InitializeComponent();
        DatePickerCtl.Date = DateTimeOffset.Now;
        KindCombo.SelectedIndex = 3;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(x => x.Date).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as WorkLogEntry;
        if (_selected is null) return;
        DatePickerCtl.Date = _selected.Date;
        KindCombo.SelectedItem = _selected.Kind;
        SpaceBox.Text = _selected.SpaceLabel;
        NoteBox.Text = _selected.Note;
        ByBox.Text = _selected.ByWhom;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new WorkLogEntry();
        item.Date = DatePickerCtl.Date;
        item.Kind = KindCombo.SelectedItem as string ?? "Other";
        item.SpaceLabel = SpaceBox.Text.Trim();
        item.Note = NoteBox.Text.Trim();
        item.ByWhom = ByBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(x => x.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
    }

    private void ClearForm()
    {
        SpaceBox.Text = string.Empty;
        NoteBox.Text = string.Empty;
        ByBox.Text = string.Empty;
    }
}
