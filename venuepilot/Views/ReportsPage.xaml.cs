using VenuePilot.Models;
using VenuePilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace VenuePilot.Views;

public sealed partial class ReportsPage : Page
{
    private readonly JsonListStore<VenueEvent> _store = new("events.json");
    private readonly JsonListStore<VenuePayment> _paymentStore = new("payments.json");

    public ReportsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var events = await _store.LoadAsync();
        var payments = await _paymentStore.LoadAsync();

        var live = events.Where(x => x.Status is "Contracted" or "Completed").ToList();
        var received = payments.Where(p => p.Kind != "Refund").Sum(p => p.Amount) -
                       payments.Where(p => p.Kind == "Refund").Sum(p => p.Amount);
        SummaryText.Text =
            $"Booked package value (contracted + completed): {live.Sum(x => x.PackagePrice):C} - cash received to date: {received:C}" +
            $"{Environment.NewLine}Outstanding balances on live events: {live.Sum(x => x.PackagePrice) - live.Sum(x => payments.Where(p => p.EventId == x.Id && p.Kind != "Refund").Sum(p => p.Amount) - payments.Where(p => p.EventId == x.Id && p.Kind == "Refund").Sum(p => p.Amount)):C}" +
            $"{Environment.NewLine}Cancelled events on record: {events.Count(x => x.Status == "Cancelled")}";

        MonthList.ItemsSource = live.GroupBy(x => x.EventDate.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => $"{g.Key}: {g.Count()} event(s) - {g.Sum(x => x.PackagePrice):C}")
            .ToList();

        TypeList.ItemsSource = events.Where(x => x.Status != "Cancelled")
            .GroupBy(x => x.EventType).OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key}: {g.Count()} event(s)")
            .ToList();

        var inquiries = events.Count(x => x.Source.Length > 0 || x.Status == "Inquiry");
        var converted = events.Count(x => x.Status is "Contracted" or "Completed");
        FunnelText.Text =
            $"Inquiries on the book: {events.Count(x => x.Status == "Inquiry")} open - events that reached contract: {converted}" +
            (inquiries + converted > 0
                ? $" - rough conversion across the book: {100.0 * converted / Math.Max(1, inquiries + converted):0}%"
                : string.Empty) +
            $"{Environment.NewLine}Follow-up is the pipeline: an inquiry untouched for 7 days flags red below and on the Dashboard.";

        InquiryList.ItemsSource = events.Where(x => x.Status == "Inquiry")
            .OrderBy(x => x.LastContacted ?? DateTimeOffset.MinValue).ToList();
    }
}
