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

public sealed partial class ExportPage : Page
{
    private readonly JsonListStore<Trip> _tripStore = new("trips.json");
    private readonly JsonListStore<FuelPurchase> _fuelStore = new("fuel.json");
    private readonly JsonListStore<RateTable> _rateStore = new("rates.json");
    private readonly ProfileStore _profileStore = new();

    public ExportPage()
    {
        this.InitializeComponent();
        YearBox.Text = DateTimeOffset.Now.Year.ToString();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var trips = await _tripStore.LoadAsync();
        var fuels = await _fuelStore.LoadAsync();
        if (trips.Count == 0 && fuels.Count == 0)
        {
            RetentionText.Text = "No records on file yet.";
            return;
        }
        var oldest = trips.Select(t => t.Date).Concat(fuels.Select(f => f.Date)).Min();
        var years = Math.Floor((DateTimeOffset.Now - oldest).TotalDays / 365.25);
        RetentionText.Text =
            $"Oldest record on file: {oldest:MMMM d, yyyy} (about {years:0} year(s) old). " +
            (years >= 4
                ? "Your file spans the general 4-year retention window - keep exporting yearly packets and keep the originals until your base jurisdiction confirms the window has closed for each return."
                : "Your file does not yet span the general 4-year retention window - keep everything.");
    }

    private async void ExportAllButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.WindowInstance is null) return;
        var year = int.TryParse(YearBox.Text.Trim(), out var y) ? y : DateTimeOffset.Now.Year;
        var profile = await _profileStore.LoadAsync();
        var trips = (await _tripStore.LoadAsync()).Where(t => t.Date.Year == year).OrderBy(t => t.Date).ToList();
        var fuels = (await _fuelStore.LoadAsync()).Where(f => f.Date.Year == year).OrderBy(f => f.Date).ToList();
        var rates = (await _rateStore.LoadAsync()).Where(r => r.Year == year)
            .OrderBy(r => r.Quarter).ThenBy(r => r.Jurisdiction).ToList();
        var allTrips = await _tripStore.LoadAsync();
        var allFuels = await _fuelStore.LoadAsync();
        var allRates = await _rateStore.LoadAsync();

        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"ifta-audit-packet-{year}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildPacketHtml(year, profile, trips, fuels, rates, allTrips, allFuels, allRates));
        StatusText.Text = $"Audit packet exported to {file.Path}";
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string BuildPacketHtml(int year, OperatorProfile p, List<Trip> trips, List<FuelPurchase> fuels,
        List<RateTable> rates, List<Trip> allTrips, List<FuelPurchase> allFuels, List<RateTable> allRates)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>IFTA Audit Packet {year}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:12px}" +
                  "th,td{border:1px solid #bbb;padding:6px 8px;text-align:left;font-size:13px}" +
                  "th{background:#f0f0f0}.r{text-align:right}</style></head><body>");
        sb.Append($"<h1>IFTA Audit Packet - {year}</h1>");
        sb.Append($"<p><strong>Operator:</strong> {Enc(p.LegalName)}{(string.IsNullOrWhiteSpace(p.Dba) ? "" : $" dba {Enc(p.Dba)}")}<br>");
        sb.Append($"<strong>IFTA account:</strong> {Enc(p.IftaAccount)} - <strong>Base jurisdiction:</strong> {Enc(p.BaseJurisdiction)}<br>");
        sb.Append($"<strong>Unit:</strong> {Enc(p.UnitId)} - <strong>VIN:</strong> {Enc(p.Vin)} - <strong>Fuel:</strong> {Enc(p.FuelType)}</p>");

        sb.Append("<h2>Trip records</h2><table><tr><th>Date</th><th>Jurisdiction</th><th class=\"r\">Odo start</th><th class=\"r\">Odo end</th><th class=\"r\">Miles</th><th>Route / notes</th></tr>");
        foreach (var t in trips)
            sb.Append($"<tr><td>{t.Date:yyyy-MM-dd}</td><td>{Enc(t.Jurisdiction)}</td><td class=\"r\">{t.OdometerStart:N1}</td><td class=\"r\">{t.OdometerEnd:N1}</td><td class=\"r\">{t.Miles:N1}</td><td>{Enc(t.RouteNotes)}</td></tr>");
        sb.Append($"<tr><td colspan=\"4\"><strong>Total</strong></td><td class=\"r\"><strong>{trips.Sum(t => t.Miles):N1}</strong></td><td></td></tr></table>");

        sb.Append("<h2>Fuel purchases</h2><table><tr><th>Date</th><th>Jurisdiction</th><th class=\"r\">Gallons</th><th class=\"r\">Cost</th><th>Receipt</th><th>Notes</th></tr>");
        foreach (var f in fuels)
            sb.Append($"<tr><td>{f.Date:yyyy-MM-dd}</td><td>{Enc(f.Jurisdiction)}</td><td class=\"r\">{f.Gallons:N3}</td><td class=\"r\">{f.TotalCost:C}</td><td>{Enc(f.ReceiptRef)}</td><td>{Enc(f.Notes)}</td></tr>");
        sb.Append($"<tr><td colspan=\"2\"><strong>Total</strong></td><td class=\"r\"><strong>{fuels.Sum(f => f.Gallons):N3}</strong></td><td class=\"r\"><strong>{fuels.Sum(f => f.TotalCost):C}</strong></td><td></td><td></td></tr></table>");

        sb.Append("<h2>Rate entries (operator-entered)</h2><table><tr><th>Quarter</th><th>Jurisdiction</th><th class=\"r\">Rate</th><th>Confirmed</th><th>Source</th></tr>");
        foreach (var r in rates)
            sb.Append($"<tr><td>{r.Year} Q{r.Quarter}</td><td>{Enc(r.Jurisdiction)}</td><td class=\"r\">{r.RatePerGallon:C4}</td><td>{(r.Confirmed ? r.ConfirmedDate.ToString("yyyy-MM-dd") : "NOT confirmed")}</td><td>{Enc(r.Source)}</td></tr>");
        sb.Append("</table>");

        sb.Append("<h2>Quarterly worksheets</h2>");
        for (var q = 1; q <= 4; q++)
        {
            var res = WorksheetCalculator.Compute(year, q, allTrips, allFuels, allRates);
            sb.Append($"<h3>{year} Q{q}</h3><p>Total miles {res.TotalMiles:N1} - Total gallons {res.TotalGallons:N3} - Average MPG {res.AverageMpg:0.00} - Total tax due {res.TotalTaxDue:C}{(res.Complete ? "" : " (INCOMPLETE - missing rates)")}</p>");
            sb.Append("<table><tr><th>Jurisdiction</th><th class=\"r\">Miles</th><th class=\"r\">Taxable gal</th><th class=\"r\">Tax-paid gal</th><th class=\"r\">Net</th><th class=\"r\">Rate</th><th class=\"r\">Tax due</th></tr>");
            foreach (var row in res.Rows)
                sb.Append($"<tr><td>{Enc(row.Jurisdiction)}</td><td class=\"r\">{row.Miles:N1}</td><td class=\"r\">{row.TaxableGallons:N0}</td><td class=\"r\">{row.TaxPaidGallons:N3}</td><td class=\"r\">{row.NetTaxableGallons:N3}</td><td class=\"r\">{(row.Rate is { } rt ? rt.ToString("C4") : "MISSING")}</td><td class=\"r\">{(row.TaxDue is { } td ? td.ToString("C") : "-")}</td></tr>");
            sb.Append("</table>");
        }
        sb.Append($"<p>Generated {DateTime.Now:MMMM d, yyyy} by TruckPilot from the operator's local records. Calculation aid only - not a filed return.</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
