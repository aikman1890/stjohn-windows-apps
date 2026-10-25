using VenuePilot.Models;
using VenuePilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace VenuePilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<VenueEvent> _eventStore = new("events.json");
    private readonly JsonListStore<VenuePayment> _paymentStore = new("payments.json");
    private readonly VenueSettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var events = await _eventStore.LoadAsync();
        var payments = await _paymentStore.LoadAsync();

        VenueText.Text = string.IsNullOrWhiteSpace(settings.VenueName)
            ? "Set the venue in Settings. Never double-book a Saturday again."
            : settings.VenueName;

        var upcoming = events.Where(x => x.Status == "Contracted" && x.EventDate.Date >= DateTimeOffset.Now.Date).ToList();
        BookText.Text =
            $"Contracted upcoming: {upcoming.Count} - holds: {events.Count(x => x.Status == "Hold")} - inquiries: {events.Count(x => x.Status == "Inquiry")}" +
            $"{Environment.NewLine}Booked revenue ahead: {upcoming.Sum(x => x.PackagePrice):C}";

        FlagText.Text =
            $"Holds expired (still on the book): {events.Count(x => x.HoldExpired)}" +
            $"{Environment.NewLine}Inquiries untouched 7+ days: {events.Count(x => x.InquiryStale)}" +
            $"{Environment.NewLine}Balances past their due date: {events.Count(x => x.Status == "Contracted" && x.BalanceDueDate is { } b && b.Date < DateTimeOffset.Now.Date)}";

        OwedList.ItemsSource = upcoming.OrderBy(x => x.EventDate).Select(x =>
        {
            var paid = payments.Where(p => p.EventId == x.Id && p.Kind != "Refund").Sum(p => p.Amount);
            var refunds = payments.Where(p => p.EventId == x.Id && p.Kind == "Refund").Sum(p => p.Amount);
            var balance = x.PackagePrice - paid + refunds;
            return $"{x.EventDate:MMM d, yyyy} - {x.ClientNames} ({x.Space}) - package {x.PackagePrice:C} - paid {paid - refunds:C} - BALANCE {balance:C}" +
                   (x.BalanceDueDate is { } b ? $" - balance due {b:MMM d}{(b.Date < DateTimeOffset.Now.Date ? " (PAST DUE)" : string.Empty)}" : string.Empty);
        }).ToList();
    }
}
