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

public sealed partial class InvoicesPage : Page
{
    private readonly JsonListStore<RepairOrder> _orderStore = new("orders.json");
    private readonly JsonListStore<ShopInvoice> _invoiceStore = new("invoices.json");
    private readonly ShopSettingsStore _settingsStore = new();
    private List<RepairOrder> _orders = new();
    private List<ShopInvoice> _items = new();
    private ShopInvoice? _selected;

    public InvoicesPage()
    {
        this.InitializeComponent();
        PayStatusCombo.ItemsSource = new[] { "Unpaid", "Partial", "Paid" };
        PayStatusCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _orders = await _orderStore.LoadAsync();
        RoPicker.ItemsSource = _orders.Select(o => o.DisplayLine).ToList();
        if (_orders.Count > 0) RoPicker.SelectedIndex = 0;
        _items = await _invoiceStore.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(i => i.Date).ToList();
    }

    private async void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        if (RoPicker.SelectedIndex < 0 || RoPicker.SelectedIndex >= _orders.Count) return;
        var ro = _orders[RoPicker.SelectedIndex];
        var approved = ro.Lines.Where(l => l.Decision == "Approved").ToList();
        if (approved.Count == 0)
        {
            StatusText.Text = "That RO has no approved lines yet - approve work on the Approvals page first.";
            return;
        }
        var settings = await _settingsStore.LoadAsync();
        var inv = new ShopInvoice
        {
            InvoiceNumber = $"INV-{_items.Count + 1001}",
            RoId = ro.Id,
            RoLabel = ro.DisplayLine,
            CustomerLabel = ro.CustomerLabel,
            VehicleLabel = ro.VehicleLabel,
            Date = DateTimeOffset.Now,
            Lines = approved.Select(l => new ShopInvoiceLine
            {
                Description = l.Description,
                Quantity = l.Quantity,
                Rate = l.Rate,
            }).ToList(),
            SuppliesFee = settings.SuppliesFee,
            TaxPercent = settings.TaxPercent,
        };
        _items.Add(inv);
        await _invoiceStore.SaveAsync(_items);
        _selected = inv;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(i => i.Id == inv.Id);
        StatusText.Text = $"Invoice {inv.InvoiceNumber} created from RO {ro.RoNumber} - total {inv.Total:C}.";
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as ShopInvoice;
        if (_selected is null) return;
        PayStatusCombo.SelectedItem = _selected.PayStatus;
        AmountPaidBox.Text = _selected.AmountPaid.ToString("0.##");
        InvoiceSummaryText.Text =
            $"{_selected.InvoiceNumber} - {_selected.CustomerLabel} - {_selected.VehicleLabel}" +
            $"{Environment.NewLine}Subtotal {_selected.Subtotal:C} - Supplies {_selected.SuppliesFee:C} - Tax {_selected.TaxAmount:C} - Total {_selected.Total:C} - Balance {_selected.Balance:C}";
    }

    private async void SavePayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.PayStatus = PayStatusCombo.SelectedItem as string ?? "Unpaid";
        _selected.AmountPaid = decimal.TryParse(AmountPaidBox.Text.Trim().TrimStart('$'), out var v) ? v : 0m;
        if (_selected.PayStatus == "Paid" && _selected.AmountPaid == 0) _selected.AmountPaid = _selected.Total;
        await _invoiceStore.SaveAsync(_items);
        RefreshList();
        StatusText.Text = $"Payment saved on {_selected.InvoiceNumber}.";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(i => i.Id == _selected.Id);
        await _invoiceStore.SaveAsync(_items);
        _selected = null;
        InvoiceSummaryText.Text = string.Empty;
        RefreshList();
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null || App.WindowInstance is null) return;
        var settings = await _settingsStore.LoadAsync();
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"invoice-{_selected.InvoiceNumber}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildInvoiceHtml(_selected, settings));
        StatusText.Text = $"Invoice exported to {file.Path}";
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string BuildInvoiceHtml(ShopInvoice inv, ShopSettings s)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Invoice {Enc(inv.InvoiceNumber)}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}" +
                  "th{background:#f0f0f0}.r{text-align:right}.tot{font-weight:bold}</style></head><body>");
        sb.Append($"<h1>{Enc(s.ShopName)}</h1>");
        sb.Append($"<p>{Enc(s.Address)}<br>{Enc(s.Phone)}</p>");
        sb.Append($"<h2>Invoice {Enc(inv.InvoiceNumber)}</h2>");
        sb.Append($"<p><strong>Customer:</strong> {Enc(inv.CustomerLabel)}<br>");
        sb.Append($"<strong>Vehicle:</strong> {Enc(inv.VehicleLabel)}<br>");
        sb.Append($"<strong>Repair order:</strong> {Enc(inv.RoLabel)}<br>");
        sb.Append($"<strong>Date:</strong> {inv.Date:MMMM d, yyyy}</p>");
        sb.Append("<table><tr><th>Description</th><th class=\"r\">Qty/Hrs</th><th class=\"r\">Rate</th><th class=\"r\">Amount</th></tr>");
        foreach (var l in inv.Lines)
        {
            sb.Append($"<tr><td>{Enc(l.Description)}</td><td class=\"r\">{l.Quantity:0.##}</td>" +
                      $"<td class=\"r\">{l.Rate:C}</td><td class=\"r\">{l.Amount:C}</td></tr>");
        }
        sb.Append($"<tr><td colspan=\"3\" class=\"r\">Subtotal</td><td class=\"r\">{inv.Subtotal:C}</td></tr>");
        if (inv.SuppliesFee > 0)
            sb.Append($"<tr><td colspan=\"3\" class=\"r\">Shop supplies</td><td class=\"r\">{inv.SuppliesFee:C}</td></tr>");
        if (inv.TaxPercent > 0)
            sb.Append($"<tr><td colspan=\"3\" class=\"r\">Tax ({inv.TaxPercent:0.##}%)</td><td class=\"r\">{inv.TaxAmount:C}</td></tr>");
        sb.Append($"<tr class=\"tot\"><td colspan=\"3\" class=\"r\">Total</td><td class=\"r\">{inv.Total:C}</td></tr>");
        sb.Append($"<tr><td colspan=\"3\" class=\"r\">Paid</td><td class=\"r\">{inv.AmountPaid:C}</td></tr>");
        sb.Append($"<tr class=\"tot\"><td colspan=\"3\" class=\"r\">Balance due</td><td class=\"r\">{inv.Balance:C}</td></tr>");
        sb.Append("</table>");
        sb.Append("<p>Thank you for your business.</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
