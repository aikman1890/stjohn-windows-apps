using System.Net;
using System.Text;
using LienPilot.Models;
using LienPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace LienPilot.Views;

public sealed partial class ClaimPage : Page
{
    private readonly JsonListStore<ClaimLine> _lineStore = new("claimlines.json");
    private readonly JsonListStore<StorageUnit> _unitStore = new("units.json");
    private readonly FacilitySettingsStore _settingsStore = new();
    private List<StorageUnit> _units = new();
    private List<ClaimLine> _lines = new();
    private FacilitySettings _settings = new();

    public ClaimPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _settings = await _settingsStore.LoadAsync();
        _units = (await _unitStore.LoadAsync()).Where(u => u.Balance > 0).OrderBy(u => u.UnitNumber).ToList();
        UnitPicker.ItemsSource = _units.Select(u => u.DisplayLine).ToList();
        if (_units.Count > 0) UnitPicker.SelectedIndex = 0;
        PresetCombo.ItemsSource = new[]
        {
            $"Late fee - {_settings.LateFee:C}",
            $"Lien fee - {_settings.LienFee:C}",
            $"Sale / advertising fee - {_settings.SaleFee:C}",
            $"Lock cut fee - {_settings.LockCutFee:C}",
            "Custom",
        };
        PresetCombo.SelectedIndex = 0;
        _lines = await _lineStore.LoadAsync();
        RefreshLines();
    }

    private StorageUnit? PickedUnit =>
        UnitPicker.SelectedIndex >= 0 && UnitPicker.SelectedIndex < _units.Count
            ? _units[UnitPicker.SelectedIndex]
            : null;

    private void UnitPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshLines();

    private void PresetCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (PresetCombo.SelectedIndex)
        {
            case 0: DescBox.Text = "Late fee"; AmountBox.Text = _settings.LateFee.ToString("0.##"); break;
            case 1: DescBox.Text = "Lien fee"; AmountBox.Text = _settings.LienFee.ToString("0.##"); break;
            case 2: DescBox.Text = "Sale / advertising fee"; AmountBox.Text = _settings.SaleFee.ToString("0.##"); break;
            case 3: DescBox.Text = "Lock cut fee"; AmountBox.Text = _settings.LockCutFee.ToString("0.##"); break;
        }
    }

    private void RefreshLines()
    {
        var unit = PickedUnit;
        if (unit is null) { LinesList.ItemsSource = null; StatementText.Text = string.Empty; return; }
        var mine = _lines.Where(l => l.UnitId == unit.Id).OrderBy(l => l.Date).ToList();
        LinesList.ItemsSource = mine.Select(l => $"{l.Date:MMM d, yyyy} - {l.Description} - {l.Amount:C}").ToList();
        var fees = mine.Sum(l => l.Amount);
        StatementText.Text =
            $"Unit {unit.UnitNumber} - {unit.TenantName}" +
            $"{Environment.NewLine}Rent balance: {unit.Balance:C} - Itemized fees: {fees:C} - TOTAL CLAIM: {unit.Balance + fees:C}";
    }

    private async void AddLineButton_Click(object sender, RoutedEventArgs e)
    {
        var unit = PickedUnit;
        if (unit is null || string.IsNullOrWhiteSpace(DescBox.Text)) return;
        _lines.Add(new ClaimLine
        {
            UnitId = unit.Id,
            Description = DescBox.Text.Trim(),
            Amount = decimal.TryParse(AmountBox.Text.Trim().TrimStart('$'), out var v) ? v : 0m,
            Date = DateTimeOffset.Now,
        });
        await _lineStore.SaveAsync(_lines);
        DescBox.Text = string.Empty;
        AmountBox.Text = string.Empty;
        RefreshLines();
    }

    private async void RemoveLineButton_Click(object sender, RoutedEventArgs e)
    {
        var unit = PickedUnit;
        if (unit is null || LinesList.SelectedIndex < 0) return;
        var mine = _lines.Where(l => l.UnitId == unit.Id).OrderBy(l => l.Date).ToList();
        if (LinesList.SelectedIndex >= mine.Count) return;
        _lines.RemoveAll(l => l.Id == mine[LinesList.SelectedIndex].Id);
        await _lineStore.SaveAsync(_lines);
        RefreshLines();
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var unit = PickedUnit;
        if (unit is null || App.WindowInstance is null) return;
        var mine = _lines.Where(l => l.UnitId == unit.Id).OrderBy(l => l.Date).ToList();
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"claim-statement-unit-{unit.UnitNumber}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildStatementHtml(unit, mine, _settings));
        StatusText.Text = $"Statement exported to {file.Path}";
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string BuildStatementHtml(StorageUnit u, List<ClaimLine> lines, FacilitySettings s)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Itemized Claim - Unit {Enc(u.UnitNumber)}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}th{background:#f0f0f0}.r{text-align:right}.tot{font-weight:bold}</style></head><body>");
        sb.Append($"<h1>{Enc(s.FacilityName)}</h1><p>{Enc(s.Address)}<br>{Enc(s.Phone)}</p>");
        sb.Append("<h2>Itemized Statement of Claim</h2>");
        sb.Append($"<p><strong>Tenant:</strong> {Enc(u.TenantName)}<br><strong>Unit:</strong> {Enc(u.UnitNumber)}<br>" +
                  $"<strong>Monthly rate:</strong> {u.MonthlyRate:C}<br><strong>Paid through:</strong> {(u.PaidThrough is { } pt ? pt.ToString("MMMM d, yyyy") : "not recorded")}</p>");
        sb.Append("<table><tr><th>Date</th><th>Description</th><th class=\"r\">Amount</th></tr>");
        sb.Append($"<tr><td></td><td>Rent balance owed</td><td class=\"r\">{u.Balance:C}</td></tr>");
        foreach (var l in lines)
            sb.Append($"<tr><td>{l.Date:yyyy-MM-dd}</td><td>{Enc(l.Description)}</td><td class=\"r\">{l.Amount:C}</td></tr>");
        var total = u.Balance + lines.Sum(l => l.Amount);
        sb.Append($"<tr class=\"tot\"><td colspan=\"2\" class=\"r\">Total claim</td><td class=\"r\">{total:C}</td></tr>");
        sb.Append("</table>");
        sb.Append($"<p>Generated {DateTime.Now:MMMM d, yyyy} by LienPilot from the facility's own records and fee schedule.</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
