using System.Net;
using System.Text;
using FarewellPilot.Models;
using FarewellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace FarewellPilot.Views;

public sealed partial class ReportsPage : Page
{
    private readonly JsonListStore<FuneralCase> _caseStore = new("cases.json");
    private readonly JsonListStore<ContractLine> _lineStore = new("contractlines.json");
    private readonly JsonListStore<Payment> _paymentStore = new("payments.json");
    private readonly JsonListStore<InsuranceAssignment> _assignStore = new("assignments.json");
    private readonly JsonListStore<PreneedContract> _preneedStore = new("preneed.json");
    private readonly JsonListStore<DepositEntry> _depositStore = new("deposits.json");
    private string _report = string.Empty;

    public ReportsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var cases = await _caseStore.LoadAsync();
        var lines = await _lineStore.LoadAsync();
        var payments = await _paymentStore.LoadAsync();
        var assignments = await _assignStore.LoadAsync();
        var preneed = await _preneedStore.LoadAsync();
        var deposits = await _depositStore.LoadAsync();

        var sb = new StringBuilder();
        sb.AppendLine("CASES BY TYPE");
        foreach (var g in cases.GroupBy(c => c.ServiceType).OrderBy(g => g.Key))
            sb.AppendLine($"  {g.Key}: {g.Count()}");
        sb.AppendLine();
        sb.AppendLine("CASES BY MONTH (first call)");
        foreach (var g in cases.GroupBy(c => c.FirstCallDate.ToString("yyyy-MM")).OrderBy(g => g.Key))
            sb.AppendLine($"  {g.Key}: {g.Count()}");
        sb.AppendLine();
        sb.AppendLine("RECEIVABLES AGING");
        var aging = cases
            .Select(c => (Case: c, Balance: CaseMath.Balance(lines, payments, assignments, c.Id)))
            .Where(x => x.Balance > 0)
            .GroupBy(x => CaseMath.AgingBucket(x.Case));
        foreach (var g in aging.OrderBy(g => g.Key))
            sb.AppendLine($"  {g.Key}: {g.Sum(x => x.Balance):C} across {g.Count()} case(s)");
        sb.AppendLine($"  TOTAL DUE: {cases.Sum(c => Math.Max(0m, CaseMath.Balance(lines, payments, assignments, c.Id))):C}");
        sb.AppendLine();
        sb.AppendLine("PRENEED");
        var activePre = preneed.Where(p => p.Status == "Active").ToList();
        sb.AppendLine($"  Active contracts: {activePre.Count} - total {activePre.Sum(p => p.TotalAmount):C}");
        sb.AppendLine($"  Deposited: {activePre.Sum(p => deposits.Where(d => d.PreneedId == p.Id).Sum(d => d.Amount)):C}");
        sb.AppendLine($"  Undeposited: {activePre.Sum(p => p.TotalAmount - deposits.Where(d => d.PreneedId == p.Id).Sum(d => d.Amount)):C}");
        sb.AppendLine($"  Fulfilled to date: {preneed.Count(p => p.Status == "Fulfilled")}");
        sb.AppendLine();
        sb.AppendLine("CASH ADVANCES");
        var advances = lines.Where(l => l.Category == "Cash advance").ToList();
        sb.AppendLine($"  Advanced on all cases: {advances.Sum(l => l.Amount):C} across {advances.Select(l => l.CaseId).Distinct().Count()} case(s)");
        sb.AppendLine($"  Still outstanding (cases with a balance): {cases.Where(c => CaseMath.Balance(lines, payments, assignments, c.Id) > 0).Sum(c => lines.Where(l => l.CaseId == c.Id && l.Category == "Cash advance").Sum(l => l.Amount)):C}");
        _report = sb.ToString();
        ReportText.Text = _report;
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.WindowInstance is null) return;
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"farewell-report-{DateTime.Now:yyyy-MM}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        var html = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>FarewellPilot Report</title>" +
                   "<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}pre{font-family:Consolas,monospace;font-size:13px}</style></head><body>" +
                   $"<h1>Case Book Report - {DateTime.Now:MMMM yyyy}</h1><pre>{WebUtility.HtmlEncode(_report)}</pre></body></html>";
        await FileIO.WriteTextAsync(file, html);
        StatusText.Text = $"Report exported to {file.Path}";
    }
}
