using HealthPilot.Models;
using HealthPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HealthPilot.Views;

public sealed partial class MedicationsPage : Page
{
    private readonly JsonListStore<Medication> _store = new("medications.json");
    private List<Medication> _items = new();
    private Medication? _selected;

    public MedicationsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(m => m.Name).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Medication;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        DosageBox.Text = _selected.Dosage;
        ScheduleBox.Text = _selected.Schedule;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) return;
        var item = _selected ?? new Medication();
        item.Name = NameBox.Text.Trim();
        item.Dosage = DosageBox.Text.Trim();
        item.Schedule = ScheduleBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(m => m.Id == item.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(m => m.Id == _selected.Id);
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
        NameBox.Text = string.Empty;
        DosageBox.Text = string.Empty;
        ScheduleBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }
}
