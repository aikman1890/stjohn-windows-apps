using System.Net;
using System.Text;
using WellPilot.Models;
using WellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace WellPilot.Views;

public sealed partial class ClosePage : Page
{
    private readonly JsonListStore<Well> _wellStore = new("wells.json");
    private readonly JsonListStore<DeckEntry> _deckStore = new("deck.json");
    private readonly JsonListStore<ProductionMonth> _productionStore = new("production.json");
    private readonly JsonListStore<WellExpense> _expenseStore = new("expenses.json");
    private readonly JsonListStore<DistributionRun> _runStore = new("runs.json");
    private readonly JsonListStore<StatementLine> _lineStore = new("lines.json");
    private readonly WellSettingsStore _settingsStore = new();
    private List<Well> _wells = new();
    private List<DistributionRun> _runs = new();
    private DistributionRun? _selectedRun;

    public ClosePage()
    {
        this.InitializeComponent();
        MonthBox.Text = DateTimeOffset.Now.AddMonths(-1).ToString("yyyy-MM");
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _wells = await _wellStore.LoadAsync();
        WellPicker.ItemsSource = _wells.Select(w => w.DisplayLine).ToList();
        if (_wells.Count > 0) WellPicker.SelectedIndex = 0;
        _runs = await _runStore.LoadAsync();
        RefreshRuns();
    }

    private void RefreshRuns()
    {
        RunsList.ItemsSource = null;
        RunsList.ItemsSource = _runs.OrderByDescending(r => r.RunDate).ToList();
    }

    private Well? CurrentWell =>
        WellPicker.SelectedIndex >= 0 && WellPicker.SelectedIndex < _wells.Count
            ? _wells[WellPicker.SelectedIndex] : null;

    private async Task<(ProductionMonth? Prod, decimal Expenses, List<DeckEntry> Deck)> LoadInputsAsync(Well well, string month)
    {
        var production = (await _productionStore.LoadAsync())
            .FirstOrDefault(p => p.WellId == well.Id && p.Month == month);
        var expenses = (await _expenseStore.LoadAsync())
            .Where(x => x.WellId == well.Id && x.Month == month).Sum(x => x.Amount);
        var deck = (await _deckStore.LoadAsync())
            .Where(d => d.WellId == well.Id && d.Active).ToList();
        return (production, expenses, deck);
    }

    private async void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        var well = CurrentWell;
        if (well is null) return;
        var (prod, expenses, deck) = await LoadInputsAsync(well, MonthBox.Text.Trim());
        if (prod is null)
        {
            PreviewText.Text = $"No production entered for {MonthBox.Text} on this well yet.";
            return;
        }
        var deckTotal = deck.Sum(d => d.DecimalInterest);
        PreviewText.Text =
            $"{well.DisplayLine} - {MonthBox.Text}" +
            $"{Environment.NewLine}Gross {prod.GrossRevenue:C} - severance {prod.SeveranceTax:C} - deductions {prod.Deductions:C} = net {prod.NetRevenue:C}" +
            $"{Environment.NewLine}Expenses to allocate (JIB): {expenses:C}" +
            $"{Environment.NewLine}Active deck lines: {deck.Count} - deck total {deckTotal:0.00000000}" +
            (Math.Abs(deckTotal - 1m) < 0.000000005m ? string.Empty : $"{Environment.NewLine}WARNING: deck does not sum to 1.00000000 - only the decked share will be distributed.");
    }

    private async void RunButton_Click(object sender, RoutedEventArgs e)
    {
        var well = CurrentWell;
        if (well is null) return;
        var month = MonthBox.Text.Trim();
        var (prod, expenses, deck) = await LoadInputsAsync(well, month);
        if (prod is null || deck.Count == 0)
        {
            StatusText.Text = prod is null ? "No production for that month." : "The active deck is empty.";
            return;
        }
        var run = new DistributionRun
        {
            WellId = well.Id,
            WellLabel = well.DisplayLine,
            Month = month,
            RunDate = DateTimeOffset.Now,
            NetRevenue = prod.NetRevenue,
            TotalExpenses = expenses,
            DeckSnapshot = "Deck used: " + string.Join("; ", deck.Select(d =>
                $"{d.OwnerLabel} {d.InterestType} {d.DecimalInterest:0.00000000} (v{d.Version}, eff {d.EffectiveDate:yyyy-MM-dd})")),
        };
        var lines = DistributionEngine.Compute(run, deck, prod.NetRevenue, expenses);
        _runs.Add(run);
        await _runStore.SaveAsync(_runs);
        var allLines = await _lineStore.LoadAsync();
        allLines.AddRange(lines);
        await _lineStore.SaveAsync(allLines);
        _selectedRun = run;
        RefreshRuns();
        RunsList.SelectedItem = run;
        StatusText.Text = $"Run complete: {lines.Count} statement lines - net to owners {lines.Sum(l => l.NetAmount):C} (revenue {prod.NetRevenue:C} - expenses {expenses:C}).";
    }

    private async void RunsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedRun = RunsList.SelectedItem as DistributionRun;
        if (_selectedRun is null) { LinesList.ItemsSource = null; return; }
        var lines = await _lineStore.LoadAsync();
        LinesList.ItemsSource = lines.Where(l => l.RunId == _selectedRun.Id).ToList();
    }

    private async void DeleteRunButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRun is null) return;
        _runs.RemoveAll(r => r.Id == _selectedRun.Id);
        await _runStore.SaveAsync(_runs);
        var lines = await _lineStore.LoadAsync();
        lines.RemoveAll(l => l.RunId == _selectedRun.Id);
        await _lineStore.SaveAsync(lines);
        _selectedRun = null;
        RefreshRuns();
        LinesList.ItemsSource = null;
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async Task<FileSavePicker> MakePickerAsync(string name)
    {
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = name;
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        return picker;
    }

    private async void StatementsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRun is null) { StatusText.Text = "Select a run first."; return; }
        var settings = await _settingsStore.LoadAsync();
        var lines = (await _lineStore.LoadAsync()).Where(l => l.RunId == _selectedRun.Id).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Owner statements</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:20px}.stmt{border:1px solid #999;padding:18px;margin-bottom:22px}table{border-collapse:collapse}td{padding:4px 14px 4px 0;font-size:14px}.muted{color:#666;font-size:12px}</style></head><body>");
        foreach (var line in lines)
        {
            sb.AppendLine("<div class=\"stmt\">");
            sb.AppendLine($"<h1>{Esc(settings.OperatorName)} - Owner statement</h1>");
            sb.AppendLine($"<p><strong>Owner:</strong> {Esc(line.OwnerLabel)}<br/><strong>Well:</strong> {Esc(_selectedRun.WellLabel)}<br/><strong>Month:</strong> {_selectedRun.Month}<br/><strong>Interest:</strong> {Esc(line.InterestType)} {line.DecimalUsed:0.00000000}</p>");
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr><td>Revenue credit (net revenue {_selectedRun.NetRevenue:C} x your decimal)</td><td>{line.RevenueCredit:C}</td></tr>");
            sb.AppendLine($"<tr><td>Joint-interest billing (your share of {_selectedRun.TotalExpenses:C} expenses)</td><td>-{line.JibCharge:C}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Net amount</strong></td><td><strong>{line.NetAmount:C}</strong></td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("<p class=\"muted\">Math: revenue is distributed by deck decimal; expenses are allocated across working-interest owners pro-rata to their WI share. Rounding is half-even to cents; any remainder is applied to the largest interest of the class.</p>");
            sb.AppendLine("</div>");
        }
        sb.AppendLine("</body></html>");
        var file = await (await MakePickerAsync($"owner-statements-{_selectedRun.Month}")).PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Statements saved to {file.Path}";
    }

    private async void PacketButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRun is null) { StatusText.Text = "Select a run first."; return; }
        var settings = await _settingsStore.LoadAsync();
        var production = (await _productionStore.LoadAsync())
            .FirstOrDefault(p => p.WellId == _selectedRun.WellId && p.Month == _selectedRun.Month);
        var expenses = (await _expenseStore.LoadAsync())
            .Where(x => x.WellId == _selectedRun.WellId && x.Month == _selectedRun.Month).ToList();
        var lines = (await _lineStore.LoadAsync()).Where(l => l.RunId == _selectedRun.Id).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Close packet</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:21px}h2{font-size:15px;margin-top:20px}table{border-collapse:collapse;width:100%}td,th{border:1px solid #bbb;padding:5px 8px;font-size:13px;text-align:left}.muted{color:#666}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.OperatorName)} - Monthly close packet</h1>");
        sb.AppendLine($"<p><strong>Well:</strong> {Esc(_selectedRun.WellLabel)}<br/><strong>Month:</strong> {_selectedRun.Month}<br/><strong>Run:</strong> {_selectedRun.RunDate:MMM d, yyyy}</p>");
        sb.AppendLine("<h2>Production &amp; revenue (as entered from purchaser statement)</h2>");
        if (production is not null)
            sb.AppendLine($"<p>Oil {production.OilBbl:0.0} bbl - Gas {production.GasMcf:0.0} MCF - Gross {production.GrossRevenue:C} - Severance {production.SeveranceTax:C} - Deductions {production.Deductions:C} - <strong>Net {production.NetRevenue:C}</strong><br/>Purchaser ref: {Esc(production.PurchaserRef)}</p>");
        sb.AppendLine("<h2>Expenses allocated (JIB)</h2><table><tr><th>Category</th><th>Vendor</th><th>Amount</th></tr>");
        foreach (var x in expenses)
            sb.AppendLine($"<tr><td>{Esc(x.Category)}</td><td>{Esc(x.Vendor)}</td><td>{x.Amount:C}</td></tr>");
        sb.AppendLine($"</table><p><strong>Total expenses: {expenses.Sum(x => x.Amount):C}</strong></p>");
        sb.AppendLine("<h2>Deck used (snapshot)</h2>");
        sb.AppendLine($"<p>{Esc(_selectedRun.DeckSnapshot)}</p>");
        sb.AppendLine("<h2>Distribution</h2><table><tr><th>Owner</th><th>Type</th><th>Decimal</th><th>Revenue</th><th>JIB</th><th>Net</th></tr>");
        foreach (var l in lines)
            sb.AppendLine($"<tr><td>{Esc(l.OwnerLabel)}</td><td>{Esc(l.InterestType)}</td><td>{l.DecimalUsed:0.00000000}</td><td>{l.RevenueCredit:C}</td><td>{l.JibCharge:C}</td><td>{l.NetAmount:C}</td></tr>");
        sb.AppendLine($"</table><p><strong>Totals: revenue {lines.Sum(l => l.RevenueCredit):C} - JIB {lines.Sum(l => l.JibCharge):C} - net {lines.Sum(l => l.NetAmount):C}</strong></p>");
        sb.AppendLine("<p class=\"muted\">Produced by WellPilot from the operator's own records. Figures must be verified against purchaser statements and current state requirements before filing or payment.</p>");
        sb.AppendLine("</body></html>");
        var file = await (await MakePickerAsync($"close-packet-{_selectedRun.Month}")).PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Close packet saved to {file.Path}";
    }
}
