using RemodelPilot.Models;
using RemodelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RemodelPilot.Views;

public sealed partial class CostingPage : Page
{
    private readonly JsonListStore<ActualCost> _store = new("actuals.json");
    private readonly JsonListStore<Job> _jobStore = new("jobs.json");
    private readonly JsonListStore<EstimateVersion> _estimateStore = new("estimates.json");
    private readonly JsonListStore<ChangeOrder> _coStore = new("changeorders.json");
    private readonly RemodelSettingsStore _settingsStore = new();
    private List<Job> _jobs = new();
    private List<ActualCost> _items = new();

    public CostingPage()
    {
        this.InitializeComponent();
        DatePicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        CategoryCombo.ItemsSource = settings.Categories;
        if (settings.Categories.Count > 0) CategoryCombo.SelectedIndex = 0;
        _jobs = await _jobStore.LoadAsync();
        JobPicker.ItemsSource = _jobs.Select(j => j.DisplayLine).ToList();
        if (_jobs.Count > 0) JobPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshAll();
    }

    private Job? PickedJob =>
        JobPicker.SelectedIndex >= 0 && JobPicker.SelectedIndex < _jobs.Count
            ? _jobs[JobPicker.SelectedIndex]
            : null;

    private void JobPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshAll();

    private async void RefreshAll()
    {
        var job = PickedJob;
        if (job is null) { CategoryList.ItemsSource = null; ItemsList.ItemsSource = null; MarginText.Text = string.Empty; return; }
        var estimates = await _estimateStore.LoadAsync();
        var winning = estimates.FirstOrDefault(v => v.Id == job.WinningVersionId);
        var budget = JobMath.BudgetByCategory(winning);
        var actual = JobMath.ActualByCategory(_items, job.Id);
        var categories = budget.Keys.Concat(actual.Keys).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c);
        CategoryList.ItemsSource = categories.Select(c =>
        {
            var b = budget.TryGetValue(c, out var bv) ? bv : 0m;
            var a = actual.TryGetValue(c, out var av) ? av : 0m;
            var flag = b > 0 && a > b ? $" - OVERRUN by {a - b:C}" : b == 0 && a > 0 ? " - no budget in this category" : "";
            return $"{c}: budget {b:C} - actual {a:C} - remaining {b - a:C}{flag}";
        }).ToList();

        var cos = await _coStore.LoadAsync();
        var revenue = JobMath.ContractPrice(job, cos);
        var totalActual = JobMath.ActualTotal(_items, job.Id);
        var margin = revenue - totalActual;
        var pct = revenue > 0 ? margin / revenue * 100m : 0m;
        MarginText.Text = $"{job.ClientName}: contract {revenue:C} - actuals {totalActual:C} - live margin {margin:C} ({pct:0.#}%)";
        ItemsList.ItemsSource = _items.Where(a => a.JobId == job.Id).OrderByDescending(a => a.Date).ToList();
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var job = PickedJob;
        if (job is null || string.IsNullOrWhiteSpace(DescBox.Text)) return;
        _items.Add(new ActualCost
        {
            JobId = job.Id,
            Date = DatePicker.Date,
            Category = CategoryCombo.SelectedItem as string ?? "Other",
            Description = DescBox.Text.Trim(),
            Vendor = VendorBox.Text.Trim(),
            Amount = decimal.TryParse(AmountBox.Text.Trim().TrimStart('$'), out var v) ? v : 0m,
        });
        await _store.SaveAsync(_items);
        DescBox.Text = string.Empty;
        VendorBox.Text = string.Empty;
        AmountBox.Text = string.Empty;
        RefreshAll();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsList.SelectedItem is not ActualCost entry) return;
        _items.RemoveAll(a => a.Id == entry.Id);
        await _store.SaveAsync(_items);
        RefreshAll();
    }
}
