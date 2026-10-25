using CharterPilot.Models;
using CharterPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CharterPilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await new CharterSettingsStore().LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.BrokerName))
            TitleText.Text = $"{settings.BrokerName} - dashboard";

        var quotes = await new JsonListStore<Quote>("quotes.json").LoadAsync();
        var trips = await new JsonListStore<TripFile>("trips.json").LoadAsync();
        var now = DateTimeOffset.Now;

        var due = quotes.Where(q => q.IsLive).OrderBy(q => q.FollowUpDue).ToList();
        FollowText.Text = due.Count == 0 ? "No live quotes. Quiet board."
            : string.Join("\n", due.Select(q => q.DisplayLine));

        var lines = new List<string>();
        foreach (var t in trips.Where(t => t.DepartAt > now && (t.DepartAt - now).TotalHours <= 48).OrderBy(t => t.DepartAt))
            lines.Add($"Departs in {(t.DepartAt - now).TotalHours:0}h: {t.RouteLabel} for {t.ClientLabel} - {(t.IsReady ? "file READY" : $"NOT READY: missing {string.Join(", ", t.MissingItems)}")}");
        var monthTrips = trips.Where(t => t.DepartAt.ToString("yyyy-MM") == now.ToString("yyyy-MM")).ToList();
        foreach (var g in monthTrips.GroupBy(t => t.Currency))
            lines.Add($"Margin kept this month ({g.Key}): {g.Sum(t => t.MarginKept):0.00} across {g.Count()} trip(s).");
        var awaiting = quotes.Count(q => q.Status == "Sent");
        lines.Add($"Quotes out awaiting any answer: {awaiting}. Live quotes: {quotes.Count(q => q.IsLive)}.");
        TripsText.Text = string.Join("\n", lines);
    }
}
