using BayPilot.Models;
using BayPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BayPilot.Views;

public sealed partial class RemindersPage : Page
{
    private readonly JsonListStore<RepairOrder> _orderStore = new("orders.json");
    private readonly JsonListStore<Vehicle> _vehicleStore = new("vehicles.json");

    public RemindersPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var orders = await _orderStore.LoadAsync();
        var vehicles = await _vehicleStore.LoadAsync();
        var now = DateTimeOffset.Now;

        PromisedList.ItemsSource = orders
            .Where(o => o.IsOpen && o.PromisedDate is { } p && p < now)
            .OrderBy(o => o.PromisedDate)
            .Select(o => $"RO {o.RoNumber} - {o.VehicleLabel} ({o.CustomerLabel}) - promised {o.PromisedDate:MMM d, h:mm tt} - {o.Status}")
            .ToList();

        var followUps = new List<string>();
        foreach (var ro in orders)
        {
            foreach (var l in ro.Lines.Where(l => l.Decision == "Declined" && !l.FollowUpDone && l.FollowUpDate is { } f && f.Date <= now.Date))
            {
                followUps.Add($"{l.Description} - {l.Amount:C} - {ro.VehicleLabel} ({ro.CustomerLabel}) - follow-up due {l.FollowUpDate:MMM d, yyyy}");
            }
        }
        FollowUpList.ItemsSource = followUps;

        var service = new List<string>();
        foreach (var v in vehicles)
        {
            var lastDone = orders
                .Where(o => o.VehicleId == v.Id && o.Status is "Done" or "Picked up")
                .OrderByDescending(o => o.OpenedDate)
                .FirstOrDefault();
            if (lastDone is null) continue;
            if (v.ServiceIntervalMonths > 0 && lastDone.OpenedDate.AddMonths(v.ServiceIntervalMonths) <= now)
                service.Add($"{v.FullLabel} - last completed RO {lastDone.OpenedDate:MMM d, yyyy}; {v.ServiceIntervalMonths}-month shop-estimate interval reached.");
            if (v.ServiceIntervalMiles > 0 && v.Mileage >= lastDone.MileageOut + v.ServiceIntervalMiles && lastDone.MileageOut > 0)
                service.Add($"{v.FullLabel} - mileage {v.Mileage:N0} is {v.ServiceIntervalMiles:N0}+ miles past the last completed RO (shop-estimate interval).");
        }
        ServiceList.ItemsSource = service;
    }
}
