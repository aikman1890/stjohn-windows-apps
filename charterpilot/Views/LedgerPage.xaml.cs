using CharterPilot.Models;
using CharterPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CharterPilot.Views;

public sealed partial class LedgerPage : Page
{
    public LedgerPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var trips = await new JsonListStore<TripFile>("trips.json").LoadAsync();
        var quotes = await new JsonListStore<Quote>("quotes.json").LoadAsync();

        ClientList.ItemsSource = trips.GroupBy(t => t.ClientLabel).OrderBy(g => g.Key).Select(g =>
            $"{g.Key}: {string.Join(", ", g.GroupBy(t => t.Currency).Select(c => $"{c.Sum(t => t.MarginKept):0.00} {c.Key}"))} across {g.Count()} trip(s)").ToList();
        OperatorList.ItemsSource = trips.GroupBy(t => t.OperatorLabel).OrderBy(g => g.Key).Select(g =>
            $"{g.Key}: {string.Join(", ", g.GroupBy(t => t.Currency).Select(c => $"{c.Sum(t => t.MarginKept):0.00} {c.Key}"))} across {g.Count()} trip(s)").ToList();
        MonthList.ItemsSource = trips.GroupBy(t => t.DepartAt.ToString("yyyy-MM")).OrderByDescending(g => g.Key).Select(g =>
            $"{g.Key}: {string.Join(", ", g.GroupBy(t => t.Currency).Select(c => $"{c.Sum(t => t.MarginKept):0.00} {c.Key}"))}").ToList();

        var lost = quotes.Where(q => q.Status is "Lost" or "Expired").ToList();
        var lines = new List<string>();
        foreach (var g in lost.GroupBy(q => string.IsNullOrWhiteSpace(q.LostReason) ? "(no reason coded)" : q.LostReason).OrderByDescending(g => g.Count()))
        {
            lines.Add($"== {g.Key}: {g.Count()} quote(s)");
            foreach (var q in g.OrderByDescending(q => q.SentAt))
                lines.Add($"   {q.ClientLabel} - {q.FromText} -> {q.ToText} - quoted {q.PriceQuoted:0.00} {q.Currency} - sent {q.SentAt:MMM d}");
        }
        if (lines.Count == 0) lines.Add("No lost quotes recorded yet.");
        LostList.ItemsSource = lines;
    }
}
