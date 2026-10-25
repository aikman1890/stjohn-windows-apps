using BayPilot.Models;
using BayPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BayPilot.Views;

public sealed partial class VehiclesPage : Page
{
    private readonly JsonListStore<Vehicle> _store = new("vehicles.json");
    private readonly JsonListStore<Customer> _customerStore = new("customers.json");
    private readonly JsonListStore<RepairOrder> _orderStore = new("orders.json");
    private List<Vehicle> _items = new();
    private List<Customer> _customers = new();
    private Vehicle? _selected;

    public VehiclesPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _customers = await _customerStore.LoadAsync();
        CustomerPicker.ItemsSource = _customers.Select(c => c.Name).ToList();
        if (_customers.Count > 0) CustomerPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(v => v.FullLabel).ToList();
    }

    private Customer? PickedCustomer =>
        CustomerPicker.SelectedIndex >= 0 && CustomerPicker.SelectedIndex < _customers.Count
            ? _customers[CustomerPicker.SelectedIndex]
            : null;

    private async void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Vehicle;
        if (_selected is null) return;
        var idx = _customers.FindIndex(c => c.Id == _selected.CustomerId);
        if (idx >= 0) CustomerPicker.SelectedIndex = idx;
        YearBox.Text = _selected.Year > 0 ? _selected.Year.ToString() : string.Empty;
        MakeBox.Text = _selected.Make;
        ModelBox.Text = _selected.Model;
        VinBox.Text = _selected.Vin;
        PlateBox.Text = _selected.Plate;
        MileageBox.Text = _selected.Mileage > 0 ? _selected.Mileage.ToString() : string.Empty;
        IntervalMonthsBox.Text = _selected.ServiceIntervalMonths > 0 ? _selected.ServiceIntervalMonths.ToString() : string.Empty;
        IntervalMilesBox.Text = _selected.ServiceIntervalMiles > 0 ? _selected.ServiceIntervalMiles.ToString() : string.Empty;
        NotesBox.Text = _selected.Notes;

        var orders = (await _orderStore.LoadAsync())
            .Where(o => o.VehicleId == _selected.Id)
            .OrderByDescending(o => o.OpenedDate)
            .ToList();
        var rows = new List<string>();
        foreach (var o in orders)
        {
            rows.Add($"RO {o.RoNumber} - {o.OpenedDate:MMM d, yyyy} - {o.Status} - in {o.MileageIn:N0} / out {o.MileageOut:N0} mi");
            foreach (var l in o.Lines)
                rows.Add($"    {l.Kind}: {l.Description} - {l.Amount:C} [{l.Decision}]");
            if (!string.IsNullOrWhiteSpace(o.Recommendations))
                rows.Add($"    Recommendation: {o.Recommendations}");
        }
        HistoryList.ItemsSource = rows;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var owner = PickedCustomer;
        if (owner is null) return;
        var item = _selected ?? new Vehicle();
        item.CustomerId = owner.Id;
        item.CustomerLabel = owner.Name;
        item.Year = int.TryParse(YearBox.Text.Trim(), out var y) ? y : 0;
        item.Make = MakeBox.Text.Trim();
        item.Model = ModelBox.Text.Trim();
        item.Vin = VinBox.Text.Trim();
        item.Plate = PlateBox.Text.Trim();
        item.Mileage = int.TryParse(MileageBox.Text.Trim(), out var m) ? m : 0;
        item.ServiceIntervalMonths = int.TryParse(IntervalMonthsBox.Text.Trim(), out var im) ? im : 0;
        item.ServiceIntervalMiles = int.TryParse(IntervalMilesBox.Text.Trim(), out var imi) ? imi : 0;
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(v => v.Id == item.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(v => v.Id == _selected.Id);
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
        YearBox.Text = string.Empty;
        MakeBox.Text = string.Empty;
        ModelBox.Text = string.Empty;
        VinBox.Text = string.Empty;
        PlateBox.Text = string.Empty;
        MileageBox.Text = string.Empty;
        IntervalMonthsBox.Text = string.Empty;
        IntervalMilesBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        HistoryList.ItemsSource = null;
    }
}
