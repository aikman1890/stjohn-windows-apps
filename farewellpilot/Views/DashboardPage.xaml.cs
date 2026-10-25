using FarewellPilot.Models;
using FarewellPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FarewellPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<FuneralCase> _caseStore = new("cases.json");
    private readonly JsonListStore<ContractLine> _lineStore = new("contractlines.json");
    private readonly JsonListStore<Payment> _paymentStore = new("payments.json");
    private readonly JsonListStore<InsuranceAssignment> _assignmentStore = new("assignments.json");
    private readonly JsonListStore<PreneedContract> _preneedStore = new("preneed.json");
    private readonly JsonListStore<DepositEntry> _depositStore = new("deposits.json");
    private readonly JsonListStore<AftercareTask> _aftercareStore = new("aftercare.json");
    private readonly FarewellSettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var cases = await _caseStore.LoadAsync();
        var lines = await _lineStore.LoadAsync();
        var payments = await _paymentStore.LoadAsync();
        var assignments = await _assignmentStore.LoadAsync();
        var preneed = await _preneedStore.LoadAsync();
        var deposits = await _depositStore.LoadAsync();
        var aftercare = await _aftercareStore.LoadAsync();

        HomeText.Text = string.IsNullOrWhiteSpace(settings.HomeName)
            ? "Set the home and your paperwork windows in Settings."
            : settings.HomeName;

        var open = cases.Where(c => c.Status != "Closed").ToList();
        CasesText.Text = string.Join(Environment.NewLine,
            open.GroupBy(c => c.Status).Select(g => $"{g.Key}: {g.Count()}")) +
            $"{Environment.NewLine}Total open: {open.Count}";

        var receivables = cases
            .Select(c => CaseMath.Balance(lines, payments, assignments, c.Id))
            .Where(b => b > 0).Sum();
        var preTotal = preneed.Where(p => p.Status == "Active").Sum(p => p.TotalAmount);
        var preDeposited = preneed.Where(p => p.Status == "Active")
            .Sum(p => deposits.Where(d => d.PreneedId == p.Id).Sum(d => d.Amount));
        MoneyText.Text =
            $"Family balances due: {receivables:C}" +
            $"{Environment.NewLine}Preneed active: {preTotal:C} - deposited: {preDeposited:C} - undeposited: {preTotal - preDeposited:C}";

        var clocks = new List<string>();
        var weekEnd = DateTimeOffset.Now.Date.AddDays(7);
        foreach (var c in cases.Where(c => c.DcFiledDate is null && c.Status != "Closed"))
        {
            var start = c.DateOfDeath ?? c.FirstCallDate;
            var due = start.Date.AddDays(settings.DcFilingDays);
            if (due <= weekEnd)
                clocks.Add($"{c.CaseNumber} {c.DecedentName}: death certificate filing due {due:MMM d, yyyy} ({settings.DcFilingDays} days from {(c.DateOfDeath is not null ? "date of death" : "first call")} per your Settings){(due < DateTimeOffset.Now.Date ? " - OVERDUE" : "")}");
        }
        foreach (var p in preneed.Where(p => p.Status == "Active"))
        {
            var deposited = deposits.Where(d => d.PreneedId == p.Id).Sum(d => d.Amount);
            var remaining = p.TotalAmount - deposited;
            var due = p.Date.Date.AddDays(settings.PreneedDepositDays);
            if (remaining > 0 && due <= weekEnd)
                clocks.Add($"{p.Number} ({p.Beneficiary}): {remaining:C} of preneed funds not yet deposited - window closes {due:MMM d, yyyy} ({settings.PreneedDepositDays} days from contract per your Settings){(due < DateTimeOffset.Now.Date ? " - OVERDUE" : "")}");
        }
        ClocksList.ItemsSource = clocks.Count > 0 ? clocks : new List<string> { "No clocks overdue or due this week." };

        AftercareList.ItemsSource = aftercare
            .Where(t => !t.Done && t.DueDate.Date <= weekEnd)
            .OrderBy(t => t.DueDate).ToList();
    }
}
