using WellPilot.Models;
using WellPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WellPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<Well> _wellStore = new("wells.json");
    private readonly JsonListStore<DistributionRun> _runStore = new("runs.json");
    private readonly JsonListStore<StatementLine> _lineStore = new("lines.json");
    private readonly JsonListStore<SuspenseEntry> _suspenseStore = new("suspense.json");
    private readonly JsonListStore<ComplianceDate> _calendarStore = new("calendar.json");
    private readonly WellSettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var wells = await _wellStore.LoadAsync();
        var runs = await _runStore.LoadAsync();
        var lines = await _lineStore.LoadAsync();
        var suspense = await _suspenseStore.LoadAsync();
        var calendar = await _calendarStore.LoadAsync();

        OperatorText.Text = string.IsNullOrWhiteSpace(settings.OperatorName)
            ? "Set the operator in Settings. Decks, JIB, revenue distribution - the monthly close, on your machine."
            : settings.OperatorName;

        var lastRun = runs.OrderByDescending(r => r.RunDate).FirstOrDefault();
        WellText.Text =
            $"Wells: {wells.Count} ({wells.Count(x => x.Status == "Producing")} producing, {wells.Count(x => x.Status == "Shut-in")} shut-in, {wells.Count(x => x.Status == "Plugged")} plugged)" +
            (lastRun is null ? $"{Environment.NewLine}No distribution run yet." :
                $"{Environment.NewLine}Last close: {lastRun.Month} - {lastRun.WellLabel} - net revenue {lastRun.NetRevenue:C}, expenses {lastRun.TotalExpenses:C}");

        var jibTotal = lines.Sum(l => l.JibCharge);
        var revTotal = lines.Sum(l => l.RevenueCredit);
        OwnerText.Text =
            $"All runs: revenue distributed {revTotal:C} - JIB billed {jibTotal:C}" +
            $"{Environment.NewLine}In suspense now: {suspense.Where(s => !s.Released).Sum(s => s.Amount):C} across {suspense.Count(s => !s.Released)} hold(s)";

        DueList.ItemsSource = calendar.OrderBy(c => c.DueDate).Take(8).ToList();
    }
}
