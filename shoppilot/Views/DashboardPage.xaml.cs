using ShopPilot.Models;
using ShopPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ShopPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<ShopQuote> _quoteStore = new("quotes.json");
    private readonly JsonListStore<ShopJob> _jobStore = new("jobs.json");
    private readonly JsonListStore<JobOperation> _opStore = new("operations.json");
    private readonly JsonListStore<OutsidePO> _poStore = new("outsidepos.json");
    private readonly ShopSettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var quotes = await _quoteStore.LoadAsync();
        var jobs = await _jobStore.LoadAsync();
        var ops = await _opStore.LoadAsync();
        var pos = await _poStore.LoadAsync();

        ShopText.Text = string.IsNullOrWhiteSpace(settings.ShopName)
            ? "Set the shop in Settings. Quote it, win it, run it on the board, print the traveler, cost it when it ships."
            : settings.ShopName;

        var open = jobs.Where(j => j.Status is "Open" or "In process" or "Complete").ToList();
        FloorText.Text =
            $"Open jobs: {open.Count} - overdue: {open.Count(j => j.DaysToDue < 0)} - due in 2 days: {open.Count(j => j.DaysToDue is >= 0 and <= 2)}" +
            $"{Environment.NewLine}Outside services out: {pos.Count(p => p.ReturnedDate is null)} - overdue returns: {pos.Count(p => p.Overdue)}";

        double Rate(int days)
        {
            var decided = quotes.Where(q => q.Date >= DateTimeOffset.Now.AddDays(-days) && q.Status is "Won" or "Lost").ToList();
            return decided.Count == 0 ? -1 : 100.0 * decided.Count(q => q.Status == "Won") / decided.Count;
        }
        var r30 = Rate(30);
        var r90 = Rate(90);
        var closed = jobs.Where(j => j.Status == "Closed").ToList();
        decimal Margin(ShopJob j)
        {
            var labor = ops.Where(o => o.JobId == j.Id).Sum(o => o.ActualHours) * settings.LaborRate;
            var outside = pos.Where(p => p.JobId == j.Id).Sum(p => p.Cost);
            return j.QuotedPrice - (j.MaterialCost + labor + outside);
        }
        MoneyText.Text =
            $"Win rate: last 30 days {(r30 < 0 ? "no decisions" : r30.ToString("0") + "%")} - 90 days {(r90 < 0 ? "no decisions" : r90.ToString("0") + "%")}" +
            $"{Environment.NewLine}Open quotes: {quotes.Count(q => q.Status == "Open")} worth {quotes.Where(q => q.Status == "Open").Sum(q => q.QuotedPrice):C}" +
            $"{Environment.NewLine}Closed jobs: {closed.Count} - total margin {closed.Sum(Margin):C}" +
            (closed.Count > 0 ? $" - average margin {closed.Sum(Margin) / closed.Count:C}" : string.Empty);

        DueList.ItemsSource = open.Where(j => j.DaysToDue <= 7).OrderBy(j => j.DueDate).ToList();
        QuoteList.ItemsSource = quotes.Where(q => q.Status == "Open").OrderBy(q => q.Date).ToList();
    }
}
