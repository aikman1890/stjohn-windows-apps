using LienPilot.Models;
using LienPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LienPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<StorageUnit> _unitStore = new("units.json");
    private readonly JsonListStore<StateProfile> _profileStore = new("profiles.json");
    private readonly JsonListStore<LienLedgerEntry> _ledgerStore = new("ledger.json");
    private readonly FacilitySettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var units = await _unitStore.LoadAsync();
        var profiles = await _profileStore.LoadAsync();
        var ledger = await _ledgerStore.LoadAsync();
        var profile = profiles.FirstOrDefault(p => p.Id == settings.ActiveProfileId) ?? profiles.FirstOrDefault();

        FacilityText.Text = string.IsNullOrWhiteSpace(settings.FacilityName)
            ? "Set the facility and your state profile in Settings - the clock engine runs on your confirmed profile."
            : $"{settings.FacilityName} - profile: {(profile is null ? "none" : profile.Name)}";

        var stages = new[] { "Late", "Lien pending", "Notice sent", "Advertised", "Sale scheduled" };
        var dollarLines = stages.Select(s =>
        {
            var sum = units.Where(u => u.Status == s).Sum(u => u.Balance);
            var count = units.Count(u => u.Status == s);
            return $"{s}: {sum:C} across {count} unit(s)";
        }).ToList();
        var totalDelinquent = units.Where(u => u.IsDelinquent).Sum(u => u.Balance);
        dollarLines.Add($"TOTAL delinquent: {totalDelinquent:C}");
        DollarsText.Text = string.Join(Environment.NewLine, dollarLines);

        UnitsText.Text = string.Join(Environment.NewLine,
            units.GroupBy(u => u.Status).OrderByDescending(g => g.Count())
                .Select(g => $"{g.Key}: {g.Count()}"));

        var steps = new List<string>();
        var weekEnd = DateTimeOffset.Now.Date.AddDays(7);
        if (profile is not null)
        {
            foreach (var u in units.Where(u => u.IsDelinquent && u.DefaultDate is not null))
            {
                var tl = LienClock.Compute(u, profile, ledger);
                foreach (var m in tl.Milestones.Where(m => !m.Done && m.Date is { } d && d.Date <= weekEnd))
                    steps.Add($"Unit {u.UnitNumber} ({u.TenantName}): {m.Label} - {m.Basis}");
                if (tl.EarliestSaleDate is { } esd && esd.Date <= weekEnd && !tl.SaleReady)
                    steps.Add($"Unit {u.UnitNumber} ({u.TenantName}): sale window opens {esd:MMM d} but BLOCKED - {string.Join(" ", tl.BlockReasons)}");
                else if (tl.SaleReady)
                    steps.Add($"Unit {u.UnitNumber} ({u.TenantName}): SALE READY per your profile - {tl.SaleBasis}");
            }
        }
        StepsList.ItemsSource = steps.Count > 0 ? steps : new List<string> { "No steps due this week." };

        SalesList.ItemsSource = units
            .Where(u => u.SaleDate is not null || u.Status == "Sale scheduled")
            .OrderBy(u => u.SaleDate)
            .Select(u => $"Unit {u.UnitNumber} - {u.TenantName} - {(u.SaleDate is { } sd ? sd.ToString("MMM d, yyyy") : "date not set")} - claim balance {u.Balance:C}")
            .ToList();
    }
}
