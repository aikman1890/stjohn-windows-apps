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

public sealed partial class ContractsPage : Page
{
    private readonly JsonListStore<ContractLine> _lineStore = new("contractlines.json");
    private readonly JsonListStore<Payment> _paymentStore = new("payments.json");
    private readonly JsonListStore<InsuranceAssignment> _assignStore = new("assignments.json");
    private readonly JsonListStore<FuneralCase> _caseStore = new("cases.json");
    private readonly FarewellSettingsStore _settingsStore = new();
    private List<FuneralCase> _cases = new();
    private List<ContractLine> _lines = new();
    private List<Payment> _payments = new();
    private List<InsuranceAssignment> _assignments = new();

    private static readonly string[] AssignFlow = { "Not filed", "Filed", "Pending", "Paid" };

    public ContractsPage()
    {
        this.InitializeComponent();
        CategoryCombo.ItemsSource = new[] { "Professional services", "Facilities", "Merchandise", "Cash advance", "Other" };
        CategoryCombo.SelectedIndex = 0;
        AssignStatusCombo.ItemsSource = new[] { "Not filed", "Filed", "Pending", "Paid", "Denied" };
        AssignStatusCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _cases = await _caseStore.LoadAsync();
        CasePicker.ItemsSource = _cases.Select(c => c.DisplayLine).ToList();
        if (_cases.Count > 0) CasePicker.SelectedIndex = 0;
        _lines = await _lineStore.LoadAsync();
        _payments = await _paymentStore.LoadAsync();
        _assignments = await _assignStore.LoadAsync();
        RefreshAll();
    }

    private FuneralCase? PickedCase =>
        CasePicker.SelectedIndex >= 0 && CasePicker.SelectedIndex < _cases.Count
            ? _cases[CasePicker.SelectedIndex]
            : null;

    private void CasePicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshAll();

    private void RefreshAll()
    {
        var c = PickedCase;
        LinesList.ItemsSource = c is null ? null : _lines.Where(l => l.CaseId == c.Id).ToList();
        PaymentsList.ItemsSource = c is null ? null : _payments.Where(p => p.CaseId == c.Id).OrderByDescending(p => p.Date).ToList();
        AssignList.ItemsSource = c is null ? null : _assignments.Where(a => a.CaseId == c.Id).ToList();
        BalanceText.Text = c is null
            ? string.Empty
            : $"{c.CaseNumber} {c.DecedentName}: contract {CaseMath.ContractTotal(_lines, c.Id):C} - paid {CaseMath.PaidTotal(_payments, _assignments, c.Id):C} - BALANCE {CaseMath.Balance(_lines, _payments, _assignments, c.Id):C}";

        AgingList.ItemsSource = _cases
            .Select(x => (Case: x, Balance: CaseMath.Balance(_lines, _payments, _assignments, x.Id)))
            .Where(x => x.Balance > 0)
            .GroupBy(x => CaseMath.AgingBucket(x.Case))
            .OrderBy(g => g.Key)
            .Select(g => $"{g.Key}: {g.Sum(x => x.Balance):C} across {g.Count()} case(s)")
            .ToList();
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private async void AddLineButton_Click(object sender, RoutedEventArgs e)
    {
        var c = PickedCase;
        if (c is null || string.IsNullOrWhiteSpace(LineDescBox.Text)) return;
        _lines.Add(new ContractLine
        {
            CaseId = c.Id,
            Category = CategoryCombo.SelectedItem as string ?? "Other",
            Description = LineDescBox.Text.Trim(),
            Amount = ParseDec(LineAmountBox.Text),
        });
        await _lineStore.SaveAsync(_lines);
        LineDescBox.Text = string.Empty;
        LineAmountBox.Text = string.Empty;
        RefreshAll();
    }

    private async void RemoveLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (LinesList.SelectedItem is not ContractLine line) return;
        _lines.RemoveAll(l => l.Id == line.Id);
        await _lineStore.SaveAsync(_lines);
        RefreshAll();
    }

    private async void AddPayButton_Click(object sender, RoutedEventArgs e)
    {
        var c = PickedCase;
        if (c is null) return;
        _payments.Add(new Payment
        {
            CaseId = c.Id,
            Date = DateTimeOffset.Now,
            Amount = ParseDec(PayAmountBox.Text),
            Method = PayMethodBox.Text.Trim(),
            Note = PayNoteBox.Text.Trim(),
        });
        await _paymentStore.SaveAsync(_payments);
        PayAmountBox.Text = string.Empty;
        PayMethodBox.Text = string.Empty;
        PayNoteBox.Text = string.Empty;
        RefreshAll();
    }

    private async void RemovePayButton_Click(object sender, RoutedEventArgs e)
    {
        if (PaymentsList.SelectedItem is not Payment pay) return;
        _payments.RemoveAll(p => p.Id == pay.Id);
        await _paymentStore.SaveAsync(_payments);
        RefreshAll();
    }

    private async void AddAssignButton_Click(object sender, RoutedEventArgs e)
    {
        var c = PickedCase;
        if (c is null || string.IsNullOrWhiteSpace(InsurerBox.Text)) return;
        var status = AssignStatusCombo.SelectedItem as string ?? "Not filed";
        _assignments.Add(new InsuranceAssignment
        {
            CaseId = c.Id,
            CaseLabel = c.DisplayLine,
            Insurer = InsurerBox.Text.Trim(),
            PolicyRef = PolicyBox.Text.Trim(),
            Amount = ParseDec(AssignAmountBox.Text),
            Status = status,
            FiledDate = status != "Not filed" ? DateTimeOffset.Now : null,
            PaidDate = status == "Paid" ? DateTimeOffset.Now : null,
        });
        await _assignStore.SaveAsync(_assignments);
        InsurerBox.Text = string.Empty;
        PolicyBox.Text = string.Empty;
        AssignAmountBox.Text = string.Empty;
        RefreshAll();
    }

    private async void AdvanceAssignButton_Click(object sender, RoutedEventArgs e)
    {
        if (AssignList.SelectedItem is not InsuranceAssignment a) return;
        var idx = Array.IndexOf(AssignFlow, a.Status);
        if (idx < 0 || idx >= AssignFlow.Length - 1) { StatusText.Text = "That assignment cannot advance further (Denied or already Paid)."; return; }
        a.Status = AssignFlow[idx + 1];
        if (a.Status == "Filed" && a.FiledDate is null) a.FiledDate = DateTimeOffset.Now;
        if (a.Status == "Paid") a.PaidDate = DateTimeOffset.Now;
        await _assignStore.SaveAsync(_assignments);
        StatusText.Text = $"Assignment now {a.Status}.";
        RefreshAll();
    }

    private async void StatementButton_Click(object sender, RoutedEventArgs e)
    {
        var c = PickedCase;
        if (c is null || App.WindowInstance is null) return;
        var settings = await _settingsStore.LoadAsync();
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"statement-{c.CaseNumber}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildStatementHtml(c, settings));
        StatusText.Text = $"Statement exported to {file.Path}";
    }

    private string BuildStatementHtml(FuneralCase c, FarewellSettings s)
    {
        var lines = _lines.Where(l => l.CaseId == c.Id).ToList();
        var pays = _payments.Where(p => p.CaseId == c.Id).OrderBy(p => p.Date).ToList();
        var assigns = _assignments.Where(a => a.CaseId == c.Id).ToList();
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Statement - {WebUtility.HtmlEncode(c.CaseNumber)}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}th{background:#f0f0f0}.r{text-align:right}.tot{font-weight:bold}</style></head><body>");
        sb.Append($"<h1>{WebUtility.HtmlEncode(s.HomeName)}</h1><p>{WebUtility.HtmlEncode(s.Address)}<br>{WebUtility.HtmlEncode(s.Phone)}</p>");
        sb.Append($"<h2>Statement of Funeral Goods and Services - Case {WebUtility.HtmlEncode(c.CaseNumber)}</h2>");
        sb.Append($"<p><strong>Decedent:</strong> {WebUtility.HtmlEncode(c.DecedentName)}<br><strong>Family:</strong> {WebUtility.HtmlEncode(c.NextOfKin)}<br><strong>Service:</strong> {WebUtility.HtmlEncode(c.ServiceType)}{(c.ServiceDate is { } sd ? $" - {sd:MMMM d, yyyy}" : "")}</p>");
        sb.Append("<table><tr><th>Category</th><th>Description</th><th class=\"r\">Amount</th></tr>");
        foreach (var l in lines)
            sb.Append($"<tr><td>{WebUtility.HtmlEncode(l.Category)}</td><td>{WebUtility.HtmlEncode(l.Description)}</td><td class=\"r\">{l.Amount:C}</td></tr>");
        sb.Append($"<tr class=\"tot\"><td colspan=\"2\" class=\"r\">Total</td><td class=\"r\">{CaseMath.ContractTotal(_lines, c.Id):C}</td></tr>");
        sb.Append("</table>");
        sb.Append("<h3>Payments and assignments</h3><table><tr><th>Date</th><th>Detail</th><th class=\"r\">Amount</th></tr>");
        foreach (var p in pays)
            sb.Append($"<tr><td>{p.Date:yyyy-MM-dd}</td><td>Payment - {WebUtility.HtmlEncode(p.Method)}</td><td class=\"r\">{p.Amount:C}</td></tr>");
        foreach (var a in assigns)
            sb.Append($"<tr><td>{(a.PaidDate ?? a.FiledDate)?.ToString("yyyy-MM-dd") ?? ""}</td><td>Insurance assignment - {WebUtility.HtmlEncode(a.Insurer)} ({WebUtility.HtmlEncode(a.Status)})</td><td class=\"r\">{(a.Status == "Paid" ? a.Amount.ToString("C") : "-")}</td></tr>");
        sb.Append($"<tr class=\"tot\"><td colspan=\"2\" class=\"r\">Balance due</td><td class=\"r\">{CaseMath.Balance(_lines, _payments, _assignments, c.Id):C}</td></tr>");
        sb.Append("</table>");
        sb.Append($"<p>Generated {DateTime.Now:MMMM d, yyyy} by FarewellPilot from the case record.</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
