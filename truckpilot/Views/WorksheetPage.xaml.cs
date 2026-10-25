using System.Net;
using System.Text;
using TruckPilot.Models;
using TruckPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace TruckPilot.Views;

public sealed partial class WorksheetPage : Page
{
    private readonly JsonListStore<Trip> _tripStore = new("trips.json");
    private readonly JsonListStore<FuelPurchase> _fuelStore = new("fuel.json");
    private readonly JsonListStore<RateTable> _rateStore = new("rates.json");
    private readonly ProfileStore _profileStore = new();
    private WorksheetCalculator.Result? _result;
    private int _year;
    private int _quarter;

    public WorksheetPage()
    {
        this.InitializeComponent();
        var now = DateTimeOffset.Now;
        _year = now.Year;
        _quarter = (now.Month - 1) / 3 + 1;
        QuarterCombo.ItemsSource = new[] { "Q1", "Q2", "Q3", "Q4" };
        QuarterCombo.SelectedIndex = _quarter - 1;
        YearBox.Text = _year.ToString();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _ = ComputeAsync();
    }

    private void ComputeButton_Click(object sender, RoutedEventArgs e) => _ = ComputeAsync();

    private async Task ComputeAsync()
    {
        _year = int.TryParse(YearBox.Text.Trim(), out var y) ? y : DateTimeOffset.Now.Year;
        _quarter = QuarterCombo.SelectedIndex + 1;
        var trips = await _tripStore.LoadAsync();
        var fuels = await _fuelStore.LoadAsync();
        var rates = await _rateStore.LoadAsync();
        _result = WorksheetCalculator.Compute(_year, _quarter, trips, fuels, rates);

        SummaryText.Text =
            $"{_year} Q{_quarter}: total miles {_result.TotalMiles:N1} - total gallons {_result.TotalGallons:N3} - average MPG {_result.AverageMpg:0.00} - " +
            (_result.Complete ? $"total tax due {_result.TotalTaxDue:C}" : "total tax due: incomplete (missing rates below)");
        RowsList.ItemsSource = _result.Rows.Select(r =>
            $"{r.Jurisdiction}: {r.Miles:N1} mi - taxable {r.TaxableGallons:N0} gal - tax-paid {r.TaxPaidGallons:N3} gal - net {r.NetTaxableGallons:N3} gal" +
            (r.Rate is { } rate ? $" @ {rate:C4}/gal = {(r.TaxDue ?? 0m):C}" : " - NO CONFIRMED RATE")).ToList();
        MissingText.Text = _result.Complete
            ? string.Empty
            : $"Missing confirmed rates for: {string.Join(", ", _result.MissingRates)}. Enter them on the Rates page from the official IFTA rate chart - TruckPilot never fills a rate in for you.";
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_result is null || App.WindowInstance is null) { await ComputeAsync(); if (_result is null) return; }
        var profile = await _profileStore.LoadAsync();
        var rates = await _rateStore.LoadAsync();
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"ifta-worksheet-{_year}-q{_quarter}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildWorksheetHtml(_result, _year, _quarter, profile, rates));
        StatusText.Text = $"Worksheet exported to {file.Path}";
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string BuildWorksheetHtml(WorksheetCalculator.Result r, int year, int quarter, OperatorProfile p, List<RateTable> rates)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>IFTA Worksheet {year} Q{quarter}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}" +
                  "th{background:#f0f0f0}.r{text-align:right}.tot{font-weight:bold}.warn{color:#b06000}</style></head><body>");
        sb.Append($"<h1>IFTA Quarterly Worksheet - {year} Q{quarter}</h1>");
        sb.Append($"<p><strong>Operator:</strong> {Enc(p.LegalName)}{(string.IsNullOrWhiteSpace(p.Dba) ? "" : $" dba {Enc(p.Dba)}")}<br>");
        sb.Append($"<strong>IFTA account:</strong> {Enc(p.IftaAccount)} - <strong>Base jurisdiction:</strong> {Enc(p.BaseJurisdiction)}<br>");
        sb.Append($"<strong>Unit:</strong> {Enc(p.UnitId)} - <strong>VIN:</strong> {Enc(p.Vin)} - <strong>Fuel:</strong> {Enc(p.FuelType)}</p>");
        sb.Append($"<p>Total miles {r.TotalMiles:N1} - Total gallons {r.TotalGallons:N3} - Average MPG {r.AverageMpg:0.00}</p>");
        sb.Append("<table><tr><th>Jurisdiction</th><th class=\"r\">Miles</th><th class=\"r\">Taxable gal</th><th class=\"r\">Tax-paid gal</th><th class=\"r\">Net taxable gal</th><th class=\"r\">Rate</th><th class=\"r\">Tax due</th></tr>");
        foreach (var row in r.Rows)
        {
            sb.Append($"<tr><td>{Enc(row.Jurisdiction)}</td><td class=\"r\">{row.Miles:N1}</td><td class=\"r\">{row.TaxableGallons:N0}</td>" +
                      $"<td class=\"r\">{row.TaxPaidGallons:N3}</td><td class=\"r\">{row.NetTaxableGallons:N3}</td>" +
                      $"<td class=\"r\">{(row.Rate is { } rt ? rt.ToString("C4") : "MISSING")}</td>" +
                      $"<td class=\"r\">{(row.TaxDue is { } td ? td.ToString("C") : "-")}</td></tr>");
        }
        sb.Append($"<tr class=\"tot\"><td colspan=\"6\" class=\"r\">Total tax due</td><td class=\"r\">{r.TotalTaxDue:C}</td></tr>");
        sb.Append("</table>");
        if (!r.Complete)
            sb.Append($"<p class=\"warn\">INCOMPLETE: no operator-confirmed rate for {Enc(string.Join(", ", r.MissingRates))}. Those rows are unpriced - the total above excludes them.</p>");
        sb.Append("<h2>Rate table used (operator-entered and confirmed)</h2>");
        sb.Append("<table><tr><th>Quarter</th><th>Jurisdiction</th><th class=\"r\">Rate</th><th>Confirmed</th><th>Source</th></tr>");
        foreach (var rt in rates.Where(x => x.Year == year && x.Quarter == quarter).OrderBy(x => x.Jurisdiction))
        {
            sb.Append($"<tr><td>{rt.Year} Q{rt.Quarter}</td><td>{Enc(rt.Jurisdiction)}</td><td class=\"r\">{rt.RatePerGallon:C4}</td>" +
                      $"<td>{(rt.Confirmed ? rt.ConfirmedDate.ToString("MMM d, yyyy") : "NOT confirmed")}</td><td>{Enc(rt.Source)}</td></tr>");
        }
        sb.Append("</table>");
        sb.Append($"<p>Generated {DateTime.Now:MMMM d, yyyy} by TruckPilot from the operator's local records. This worksheet is a calculation aid, not a filed return. Verify all figures and rates against your base jurisdiction's current rules.</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
