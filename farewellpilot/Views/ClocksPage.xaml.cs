using FarewellPilot.Models;
using FarewellPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FarewellPilot.Views;

public sealed partial class ClocksPage : Page
{
    private readonly JsonListStore<FuneralCase> _caseStore = new("cases.json");
    private readonly JsonListStore<PreneedContract> _preneedStore = new("preneed.json");
    private readonly JsonListStore<DepositEntry> _depositStore = new("deposits.json");
    private readonly FarewellSettingsStore _settingsStore = new();

    public ClocksPage()
    {
        this.InitializeComponent();
    }

    private static string StateText(DateTimeOffset due, bool done)
    {
        if (done) return "done";
        var days = (due.Date - DateTimeOffset.Now.Date).Days;
        return days < 0 ? $"OVERDUE by {-days} day(s)" : days == 0 ? "due TODAY" : days <= 5 ? $"due in {days} day(s)" : $"due in {days} day(s)";
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var cases = await _caseStore.LoadAsync();
        var preneed = await _preneedStore.LoadAsync();
        var deposits = await _depositStore.LoadAsync();

        BasisText.Text =
            $"Death certificate window: {settings.DcFilingDays} day(s) from date of death (or first call) - your setting from your state rule. " +
            $"Preneed deposit window: {settings.PreneedDepositDays} day(s) from contract - your setting from your trust requirement. Change them in Settings; nothing here is shipped law.";

        DcList.ItemsSource = cases
            .Where(c => c.Status != "Closed")
            .OrderBy(c => c.DcFiledDate ?? (c.DateOfDeath ?? c.FirstCallDate).Date.AddDays(settings.DcFilingDays))
            .Select(c =>
            {
                var start = c.DateOfDeath ?? c.FirstCallDate;
                var due = start.Date.AddDays(settings.DcFilingDays);
                return c.DcFiledDate is { } f
                    ? $"{c.CaseNumber} {c.DecedentName}: filed {f:MMM d, yyyy} - done"
                    : $"{c.CaseNumber} {c.DecedentName}: due {due:MMM d, yyyy} - {StateText(due, false)}";
            }).ToList();

        DepositList.ItemsSource = preneed
            .Where(p => p.Status == "Active")
            .Select(p =>
            {
                var deposited = deposits.Where(d => d.PreneedId == p.Id).Sum(d => d.Amount);
                var remaining = p.TotalAmount - deposited;
                var due = p.Date.Date.AddDays(settings.PreneedDepositDays);
                return remaining <= 0
                    ? $"{p.Number} ({p.Beneficiary}): fully deposited {deposited:C} - done"
                    : $"{p.Number} ({p.Beneficiary}): {remaining:C} undeposited of {p.TotalAmount:C} - window closes {due:MMM d, yyyy} - {StateText(due, false)}";
            }).ToList();

        PermitList.ItemsSource = cases
            .Where(c => !string.IsNullOrWhiteSpace(c.PermitNote) && c.Status != "Closed")
            .Select(c => $"{c.CaseNumber} {c.DecedentName}: {c.PermitNote}")
            .ToList();
    }
}
