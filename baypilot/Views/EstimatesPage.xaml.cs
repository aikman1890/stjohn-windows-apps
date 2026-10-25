using System.Net;
using System.Text;
using BayPilot.Models;
using BayPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace BayPilot.Views;

public sealed partial class EstimatesPage : Page
{
    private readonly JsonListStore<RepairOrder> _orderStore = new("orders.json");
    private readonly ShopSettingsStore _settingsStore = new();
    private List<RepairOrder> _orders = new();
    private ShopSettings _settings = new();
    private RepairOrder? _ro;
    private List<RoLine> _editLines = new();

    public EstimatesPage()
    {
        this.InitializeComponent();
        KindCombo.ItemsSource = new[] { "Labor", "Part", "Fee" };
        KindCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _settings = await _settingsStore.LoadAsync();
        _orders = await _orderStore.LoadAsync();
        RoPicker.ItemsSource = _orders.Select(o => o.DisplayLine).ToList();
        if (_orders.Count > 0) RoPicker.SelectedIndex = 0;
        MarkupBox.Text = _settings.DefaultMarkupPercent.ToString("0.##");
    }

    private void RoPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _ro = RoPicker.SelectedIndex >= 0 && RoPicker.SelectedIndex < _orders.Count
            ? _orders[RoPicker.SelectedIndex]
            : null;
        _editLines = _ro is null ? new List<RoLine>() : new List<RoLine>(_ro.Lines);
        RoSummaryText.Text = _ro is null
            ? string.Empty
            : $"{_ro.CustomerLabel} - {_ro.VehicleLabel} - {_ro.Status}" +
              (string.IsNullOrWhiteSpace(_ro.Complaint) ? "" : $"{Environment.NewLine}{_ro.Complaint}");
        RefreshLines();
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private void RefreshLines()
    {
        LinesList.ItemsSource = null;
        LinesList.ItemsSource = _editLines;
        if (_ro is null) { TotalsText.Text = string.Empty; return; }
        var approved = _editLines.Where(l => l.Decision == "Approved").Sum(l => l.Amount);
        var all = _editLines.Sum(l => l.Amount);
        var tax = (approved + _settings.SuppliesFee) * _settings.TaxPercent / 100m;
        TotalsText.Text =
            $"Approved subtotal {approved:C}  -  Supplies {_settings.SuppliesFee:C}  -  Tax {_settings.TaxPercent:0.##}% {tax:C}  -  Estimate total {approved + _settings.SuppliesFee + tax:C}" +
            $"{Environment.NewLine}All lines (incl. pending/declined): {all:C}";
    }

    private void LaborPresetButton_Click(object sender, RoutedEventArgs e)
    {
        KindCombo.SelectedIndex = 0;
        RateBox.Text = _settings.LaborRate.ToString("0.##");
        if (string.IsNullOrWhiteSpace(DescBox.Text)) DescBox.Text = "Labor";
    }

    private void AddLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (_ro is null || string.IsNullOrWhiteSpace(DescBox.Text)) return;
        var kind = KindCombo.SelectedItem as string ?? "Labor";
        var line = new RoLine
        {
            Kind = kind,
            Description = DescBox.Text.Trim(),
            Quantity = ParseDec(QtyBox.Text) is var q && q > 0 ? q : 1m,
        };
        if (kind == "Part")
        {
            line.PartCost = ParseDec(CostBox.Text);
            line.MarkupPercent = ParseDec(MarkupBox.Text);
            line.Rate = Math.Round(line.PartCost * (1m + line.MarkupPercent / 100m), 2);
        }
        else
        {
            line.Rate = ParseDec(RateBox.Text);
        }
        _editLines.Add(line);
        DescBox.Text = string.Empty;
        QtyBox.Text = "1";
        RateBox.Text = "0";
        CostBox.Text = "0";
        RefreshLines();
    }

    private void RemoveLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (LinesList.SelectedItem is RoLine line)
        {
            _editLines.Remove(line);
            RefreshLines();
        }
    }

    private void LinesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LinesList.SelectedItem is RoLine line)
        {
            KindCombo.SelectedItem = line.Kind;
            DescBox.Text = line.Description;
            QtyBox.Text = line.Quantity.ToString("0.##");
            RateBox.Text = line.Rate.ToString("0.##");
            CostBox.Text = line.PartCost.ToString("0.##");
            MarkupBox.Text = line.MarkupPercent.ToString("0.##");
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_ro is null) return;
        _ro.Lines = _editLines;
        var orders = await _orderStore.LoadAsync();
        var stored = orders.FirstOrDefault(o => o.Id == _ro.Id);
        if (stored is not null) stored.Lines = _editLines;
        await _orderStore.SaveAsync(orders);
        _orders = orders;
        StatusText.Text = $"Estimate saved on RO {_ro.RoNumber}.";
        RefreshLines();
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_ro is null || App.WindowInstance is null) return;
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"estimate-ro-{_ro.RoNumber}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildEstimateHtml(_ro, _editLines, _settings));
        StatusText.Text = $"Estimate exported to {file.Path} - open it in a browser to print or save as PDF.";
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string BuildEstimateHtml(RepairOrder ro, List<RoLine> lines, ShopSettings s)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Estimate - RO {Enc(ro.RoNumber)}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}" +
                  "th{background:#f0f0f0}.r{text-align:right}.tot{font-weight:bold}" +
                  ".sig{margin-top:48px;border-top:1px solid #222;padding-top:6px;width:60%}</style></head><body>");
        sb.Append($"<h1>{Enc(s.ShopName)}</h1>");
        sb.Append($"<p>{Enc(s.Address)}<br>{Enc(s.Phone)}</p>");
        sb.Append($"<h2>Repair Estimate - RO {Enc(ro.RoNumber)}</h2>");
        sb.Append($"<p><strong>Customer:</strong> {Enc(ro.CustomerLabel)}<br>");
        sb.Append($"<strong>Vehicle:</strong> {Enc(ro.VehicleLabel)}<br>");
        sb.Append($"<strong>Date:</strong> {ro.OpenedDate:MMMM d, yyyy}</p>");
        if (!string.IsNullOrWhiteSpace(ro.Complaint))
            sb.Append($"<p><strong>Concern:</strong> {Enc(ro.Complaint)}</p>");
        sb.Append("<table><tr><th>Kind</th><th>Description</th><th class=\"r\">Qty/Hrs</th><th class=\"r\">Rate</th><th class=\"r\">Amount</th><th>Decision</th></tr>");
        foreach (var l in lines)
        {
            sb.Append($"<tr><td>{Enc(l.Kind)}</td><td>{Enc(l.Description)}</td><td class=\"r\">{l.Quantity:0.##}</td>" +
                      $"<td class=\"r\">{l.Rate:C}</td><td class=\"r\">{l.Amount:C}</td><td>{Enc(l.Decision)}</td></tr>");
        }
        var approved = lines.Where(l => l.Decision == "Approved").Sum(l => l.Amount);
        var tax = (approved + s.SuppliesFee) * s.TaxPercent / 100m;
        sb.Append($"<tr><td colspan=\"4\" class=\"r\">Approved subtotal</td><td class=\"r\">{approved:C}</td><td></td></tr>");
        if (s.SuppliesFee > 0)
            sb.Append($"<tr><td colspan=\"4\" class=\"r\">Shop supplies</td><td class=\"r\">{s.SuppliesFee:C}</td><td></td></tr>");
        if (s.TaxPercent > 0)
            sb.Append($"<tr><td colspan=\"4\" class=\"r\">Tax ({s.TaxPercent:0.##}%)</td><td class=\"r\">{tax:C}</td><td></td></tr>");
        sb.Append($"<tr class=\"tot\"><td colspan=\"4\" class=\"r\">Estimate total (approved work)</td><td class=\"r\">{approved + s.SuppliesFee + tax:C}</td><td></td></tr>");
        sb.Append("</table>");
        sb.Append("<p>By signing, the customer approves the work marked Approved above at the prices shown.</p>");
        sb.Append("<div class=\"sig\">Customer signature &nbsp;&nbsp;&nbsp;&nbsp; Date</div>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
