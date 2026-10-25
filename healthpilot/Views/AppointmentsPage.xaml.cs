using HealthPilot.Models;
using HealthPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HealthPilot.Views;

public sealed partial class AppointmentsPage : Page
{
    private readonly JsonListStore<AppointmentItem> _store = new("appointments.json");
    private List<AppointmentItem> _items = new();
    private AppointmentItem? _selected;

    public AppointmentsPage()
    {
        this.InitializeComponent();
        DatePicker.Date = DateTimeOffset.Now;
        TimePicker.Time = new TimeSpan(9, 0, 0);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(a => a.When).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as AppointmentItem;
        if (_selected is null) return;
        TitleBox.Text = _selected.Title;
        ProviderBox.Text = _selected.Provider;
        DatePicker.Date = _selected.When;
        TimePicker.Time = _selected.When.TimeOfDay;
        LocationBox.Text = _selected.Location;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) return;
        var item = _selected ?? new AppointmentItem();
        item.Title = TitleBox.Text.Trim();
        item.Provider = ProviderBox.Text.Trim();
        item.When = DatePicker.Date.Date + TimePicker.Time;
        item.Location = LocationBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(a => a.Id == item.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(a => a.Id == _selected.Id);
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
        TitleBox.Text = string.Empty;
        ProviderBox.Text = string.Empty;
        DatePicker.Date = DateTimeOffset.Now;
        TimePicker.Time = new TimeSpan(9, 0, 0);
        LocationBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }
}
