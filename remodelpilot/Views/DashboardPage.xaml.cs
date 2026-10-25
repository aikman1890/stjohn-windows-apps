using RemodelPilot.Models;
using RemodelPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RemodelPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<Job> _jobStore = new("jobs.json");
    private readonly JsonListStore<ChangeOrder> _coStore = new("changeorders.json");
    private readonly JsonListStore<ActualCost> _actualStore = new("actuals.json");
    private readonly JsonListStore<Draw> _drawStore = new("draws.json");
    private readonly RemodelSettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        BizText.Text = string.IsNullOrWhiteSpace(settings.BusinessName)
            ? "Set your business in Settings. Money math is decimal throughout - margins never float-round."
            : settings.BusinessName;

        var jobs = await _jobStore.LoadAsync();
        var cos = await _coStore.LoadAsync();
        var actuals = await _actualStore.LoadAsync();
        var draws = await _drawStore.LoadAsync();

        var active = jobs.Where(j => j.Status is "Won" or "In progress" or "Punch list").ToList();
        JobsList.ItemsSource = active.Select(j =>
        {
            var revenue = JobMath.ContractPrice(j, cos);
            var cost = JobMath.ActualTotal(actuals, j.Id);
            var margin = revenue - cost;
            var pct = revenue > 0 ? margin / revenue * 100m : 0m;
            return $"{j.ClientName} ({j.Status}) - contract {revenue:C} - actuals {cost:C} - margin {margin:C} ({pct:0.#}%)";
        }).ToList();

        var awaiting = cos.Where(c => c.Status == "Sent").ToList();
        CoText.Text = awaiting.Count == 0
            ? "No unsigned change orders."
            : $"{awaiting.Sum(c => c.Price):C} sitting unsigned across {awaiting.Count} change order(s) - money waiting on a signature.";

        var outstanding = draws.Where(d => d.PaidDate is null).ToList();
        var overdue = draws.Where(d => d.OverdueToInvoice).ToList();
        DrawText.Text = outstanding.Count == 0
            ? "No draws outstanding."
            : $"{outstanding.Sum(d => d.Amount):C} unpaid across {outstanding.Count} draw(s); {overdue.Count} overdue just to invoice.";

        var cutoff = DateTimeOffset.Now.Date.AddDays(60);
        WarrantyList.ItemsSource = jobs
            .Where(j => j.WarrantyEnd is { } w && w.Date >= DateTimeOffset.Now.Date && w.Date <= cutoff)
            .OrderBy(j => j.WarrantyEnd)
            .Select(j => $"{j.ClientName} - warranty ends {j.WarrantyEnd:MMM d, yyyy}")
            .ToList();
    }
}
