using BayPilot.Models;
using BayPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BayPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<Customer> _customerStore = new("customers.json");
    private readonly JsonListStore<Vehicle> _vehicleStore = new("vehicles.json");
    private readonly JsonListStore<RepairOrder> _orderStore = new("orders.json");
    private readonly JsonListStore<ShopInvoice> _invoiceStore = new("invoices.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await SampleSeed.EnsureAsync(_customerStore, _vehicleStore, _orderStore);
        var orders = await _orderStore.LoadAsync();
        var invoices = await _invoiceStore.LoadAsync();
        var today = DateTimeOffset.Now.Date;
        var tomorrow = today.AddDays(1);

        OpenedText.Text = orders.Count(o => o.OpenedDate.Date == today).ToString();
        ClosedText.Text = orders.Count(o => o.Status == "Picked up").ToString();
        InvoicedText.Text = invoices.Where(i => i.Date.Date == today).Sum(i => i.Total).ToString("C");
        CollectedText.Text = invoices.Sum(i => i.AmountPaid).ToString("C");
        PendingText.Text = orders.Count(o => o.Status == "Waiting approval").ToString();

        BoardList.ItemsSource = orders
            .Where(o => o.IsOpen)
            .OrderBy(o => o.PromisedDate ?? DateTimeOffset.MaxValue)
            .Select(o => $"RO {o.RoNumber} - {o.VehicleLabel} ({o.CustomerLabel}) - {o.Status}" +
                         (o.PromisedDate is { } p ? $" - promised {p:MMM d, h:mm tt}" : ""))
            .ToList();
        TomorrowList.ItemsSource = orders
            .Where(o => o.IsOpen && o.PromisedDate is { } p && p.Date == tomorrow)
            .OrderBy(o => o.PromisedDate)
            .Select(o => $"RO {o.RoNumber} - {o.VehicleLabel} ({o.CustomerLabel}) - promised {o.PromisedDate:h:mm tt}")
            .ToList();
    }
}
