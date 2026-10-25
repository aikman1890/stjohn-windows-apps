using BayPilot.Models;
using BayPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BayPilot.Views;

public sealed partial class BoardPage : Page
{
    private static readonly string[] Statuses =
    {
        "Checked in",
        "Diagnosing",
        "Waiting approval",
        "Approved / In progress",
        "Waiting parts",
        "Done",
        "Picked up",
    };

    private readonly JsonListStore<RepairOrder> _store = new("orders.json");
    private readonly JsonListStore<Vehicle> _vehicleStore = new("vehicles.json");
    private List<RepairOrder> _items = new();
    private List<Vehicle> _vehicles = new();
    private RepairOrder? _selected;

    public BoardPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = Statuses;
        StatusCombo.SelectedIndex = 0;
        PromisedDatePicker.Date = DateTimeOffset.Now.AddDays(1);
        PromisedTimePicker.Time = new TimeSpan(17, 0, 0);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _vehicles = await _vehicleStore.LoadAsync();
        VehiclePicker.ItemsSource = _vehicles.Select(v => v.FullLabel).ToList();
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items
            .OrderByDescending(o => o.IsOpen)
            .ThenBy(o => o.PromisedDate ?? DateTimeOffset.MaxValue)
            .ToList();
    }

    private Vehicle? PickedVehicle =>
        VehiclePicker.SelectedIndex >= 0 && VehiclePicker.SelectedIndex < _vehicles.Count
            ? _vehicles[VehiclePicker.SelectedIndex]
            : null;

    private async void VehiclePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var v = PickedVehicle;
        if (v is null) { CarriedText.Text = string.Empty; return; }
        var prior = _items
            .Where(o => o.VehicleId == v.Id && (_selected is null || o.Id != _selected.Id))
            .OrderByDescending(o => o.OpenedDate)
            .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Recommendations));
        CarriedText.Text = prior is null
            ? "No earlier recommendations on file for this vehicle."
            : $"Carried forward from RO {prior.RoNumber} ({prior.OpenedDate:MMM d, yyyy}): {prior.Recommendations}";
        await Task.CompletedTask;
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as RepairOrder;
        if (_selected is null) return;
        RoNumberBox.Text = _selected.RoNumber;
        var idx = _vehicles.FindIndex(v => v.Id == _selected.VehicleId);
        if (idx >= 0) VehiclePicker.SelectedIndex = idx;
        StatusCombo.SelectedItem = _selected.Status;
        PromisedCheck.IsChecked = _selected.PromisedDate is not null;
        if (_selected.PromisedDate is { } p)
        {
            PromisedDatePicker.Date = p;
            PromisedTimePicker.Time = p.TimeOfDay;
        }
        TechBox.Text = _selected.Tech;
        MileageInBox.Text = _selected.MileageIn > 0 ? _selected.MileageIn.ToString() : string.Empty;
        MileageOutBox.Text = _selected.MileageOut > 0 ? _selected.MileageOut.ToString() : string.Empty;
        ComplaintBox.Text = _selected.Complaint;
        RecommendBox.Text = _selected.Recommendations;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var v = PickedVehicle;
        if (v is null) return;
        var item = _selected ?? new RepairOrder();
        item.RoNumber = string.IsNullOrWhiteSpace(RoNumberBox.Text)
            ? (_items.Count + 1001).ToString()
            : RoNumberBox.Text.Trim();
        item.CustomerId = v.CustomerId;
        item.CustomerLabel = v.CustomerLabel;
        item.VehicleId = v.Id;
        item.VehicleLabel = v.DisplayLine;
        item.Status = StatusCombo.SelectedItem as string ?? Statuses[0];
        item.PromisedDate = PromisedCheck.IsChecked == true
            ? PromisedDatePicker.Date.Date + PromisedTimePicker.Time
            : null;
        item.Tech = TechBox.Text.Trim();
        item.MileageIn = int.TryParse(MileageInBox.Text.Trim(), out var mi) ? mi : 0;
        item.MileageOut = int.TryParse(MileageOutBox.Text.Trim(), out var mo) ? mo : 0;
        item.Complaint = ComplaintBox.Text.Trim();
        item.Recommendations = RecommendBox.Text.Trim();
        if (_selected is null)
        {
            item.OpenedDate = DateTimeOffset.Now;
            _items.Add(item);
        }
        await _store.SaveAsync(_items);

        // Keep the vehicle's current mileage in step with the newest RO.
        if (item.MileageOut > 0 || item.MileageIn > 0)
        {
            var stored = _vehicles.FirstOrDefault(x => x.Id == v.Id);
            if (stored is not null)
            {
                stored.Mileage = Math.Max(item.MileageOut, item.MileageIn);
                await _vehicleStore.SaveAsync(_vehicles);
            }
        }
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(o => o.Id == item.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(o => o.Id == _selected.Id);
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
        RoNumberBox.Text = string.Empty;
        StatusCombo.SelectedIndex = 0;
        PromisedCheck.IsChecked = false;
        PromisedDatePicker.Date = DateTimeOffset.Now.AddDays(1);
        PromisedTimePicker.Time = new TimeSpan(17, 0, 0);
        TechBox.Text = string.Empty;
        MileageInBox.Text = string.Empty;
        MileageOutBox.Text = string.Empty;
        ComplaintBox.Text = string.Empty;
        RecommendBox.Text = string.Empty;
        CarriedText.Text = string.Empty;
    }
}
