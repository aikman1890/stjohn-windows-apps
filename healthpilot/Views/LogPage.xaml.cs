using HealthPilot.Models;
using HealthPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HealthPilot.Views;

public sealed partial class LogPage : Page
{
    private static readonly string[] Categories = { "CPAP", "Pain", "Symptom", "Note" };

    private readonly JsonListStore<LogEntry> _store = new("healthlog.json");
    private List<LogEntry> _items = new();
    private LogEntry? _selected;

    public LogPage()
    {
        this.InitializeComponent();
        CategoryCombo.ItemsSource = Categories;
        CategoryCombo.SelectedIndex = 0;
        EntryDatePicker.Date = DateTimeOffset.Now;
        PainBox.Value = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(l => l.Date).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as LogEntry;
        if (_selected is null) return;
        EntryDatePicker.Date = _selected.Date;
        CategoryCombo.SelectedItem = _selected.Category;
        PainBox.Value = _selected.PainLevel;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new LogEntry();
        item.Date = EntryDatePicker.Date;
        item.Category = CategoryCombo.SelectedItem as string ?? Categories[0];
        item.PainLevel = (int)Math.Round(PainBox.Value);
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(l => l.Id == item.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(l => l.Id == _selected.Id);
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
        EntryDatePicker.Date = DateTimeOffset.Now;
        CategoryCombo.SelectedIndex = 0;
        PainBox.Value = 0;
        NotesBox.Text = string.Empty;
    }
}
