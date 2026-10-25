using RallyPilot.Models;
using RallyPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RallyPilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await new RallySettingsStore().LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.BusinessName))
            TitleText.Text = $"{settings.BusinessName} - group pipeline";

        var quotes = await new JsonListStore<Quote>("quotes.json").LoadAsync();
        var lines = await new JsonListStore<QuoteLine>("quotelines.json").LoadAsync();
        var sent = quotes.Where(q => q.Status == "Sent").OrderByDescending(q => q.DaysWaiting ?? 0).ToList();
        QuoteText.Text = sent.Count == 0 ? "No quotes waiting on an answer."
            : string.Join("\n", sent.Select(q =>
                $"{(q.DaysWaiting >= 3 ? "OVERDUE" : q.DaysWaiting >= 2 ? "FOLLOW UP" : "sent")} - {q.CompanyLabel} ({q.OrganizerLabel}) - {q.Heads} heads, {lines.Where(l => l.QuoteId == q.Id).Sum(l => l.LineTotal):0.00} {q.Currency} - {q.StatusLine}"));

        var events = await new JsonListStore<RallyEvent>("events.json").LoadAsync();
        var roster = await new JsonListStore<RosterEntry>("roster.json").LoadAsync();
        var upcoming = events.Where(ev => ev.Status != "Cancelled" && ev.Status != "Completed")
            .OrderBy(ev => ev.EventDate).Take(6).ToList();
        EventText.Text = upcoming.Count == 0 ? "No events in flight."
            : string.Join("\n", upcoming.Select(ev =>
            {
                var mine = roster.Where(r => r.EventId == ev.Id).ToList();
                var unsigned = mine.Count(r => !r.WaiverSigned);
                return $"{ev.EventDate:MMM d} - {ev.CompanyLabel}: {ev.ConfirmedHeads}/{ev.ExpectedHeads} heads - waivers {mine.Count - unsigned}/{mine.Count} signed{(unsigned > 0 ? $" - CHASE {unsigned}" : " - roster clear")}" +
                    (ev.BalanceDueDate is { } d ? $" - balance due {d:MMM d} ({ev.BalanceDays} days)" : string.Empty);
            }));
    }
}
