using System.Net;
using System.Text;
using KennelPilot.Models;
using KennelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace KennelPilot.Views;

public sealed partial class BillingPage : Page
{
    private readonly JsonListStore<Stay> _stayStore = new("stays.json");
    private readonly JsonListStore<KennelInvoice> _invoiceStore = new("invoices.json");
    private readonly KennelSettingsStore _settingsStore = new();
    private List<Stay> _stays = new();
    private List<KennelInvoice> _items = new();
    private KennelInvoice? _selected;

    public BillingPage()
    {
        this.InitializeComponent();
        PayStatusCombo.ItemsSource = new[] { "Unpaid", "Partial", "Paid" };
        PayStatusCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _stays = (await _stayStore.LoadAsync()).Where(s => s.Status != "Cancelled").OrderByDescending(s => s.CheckIn).ToList();
        StayPicker.ItemsSource = _stays.Select(s => s.DisplayLine).ToList();
        if (_stays.Count > 0) StayPicker.SelectedIndex = 0;
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
        if (StayPicker.SelectedIndex < 0 || StayPicker.SelectedIndex >= _stays.Count) return;
        var stay = _stays[StayPicker.SelectedIndex];
        var settings = await _settingsStore.LoadAsync();
        var sameOwnerInHouse = _stays.Count(s => s.OwnerId == stay.OwnerId && s.IsInHouse);
        var nightly = sameOwnerInHouse > 1 ? settings.SecondPetRate : settings.NightlyRate;
        var inv = new KennelInvoice
        {
            InvoiceNumber = $"K-{_items.Count + 1001}",
            StayId = stay.Id,
            PetLabel = stay.PetLabel,
            OwnerLabel = stay.OwnerLabel,
            Date = DateTimeOffset.Now,
            Lines = new List<KennelInvoiceLine>
            {
                new() { Description = $"Boarding - {stay.Nights} night(s)", Quantity = stay.Nights, Rate = nightly },
            },
        };
        if (BathCheck.IsChecked == true)
            inv.Lines.Add(new KennelInvoiceLine { Description = "Bath", Quantity = 1, Rate = settings.BathRate });
        if (MedsCheck.IsChecked == true)
            inv.Lines.Add(new KennelInvoiceLine { Description = "Medication administration", Quantity = stay.Nights, Rate = settings.MedicationRatePerDay });
        _items.Add(inv);
        await _invoiceStore.SaveAsync(_items);
        _selected = inv;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(i => i.Id == inv.Id);
        StatusText.Text = $"Invoice {inv.InvoiceNumber} created - total {inv.Total:C}. Edit rates on the invoice by deleting and recreating if needed.";
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as KennelInvoice;
        if (_selected is null) return;
        PayStatusCombo.SelectedItem = _selected.PayStatus;
        AmountPaidBox.Text = _selected.AmountPaid.ToString("0.##");
        SummaryText.Text = $"{_selected.InvoiceNumber} - {_selected.PetLabel} - total {_selected.Total:C} - balance {_selected.Balance:C}";
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
        SummaryText.Text = string.Empty;
        RefreshList();
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null || App.WindowInstance is null) return;
        var settings = await _settingsStore.LoadAsync();
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"kennel-invoice-{_selected.InvoiceNumber}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Invoice {_selected.InvoiceNumber}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}th{background:#f0f0f0}.r{text-align:right}.tot{font-weight:bold}</style></head><body>");
        sb.Append($"<h1>{WebUtility.HtmlEncode(settings.KennelName)}</h1>");
        sb.Append($"<p>{WebUtility.HtmlEncode(settings.Address)}<br>{WebUtility.HtmlEncode(settings.Phone)}</p>");
        sb.Append($"<h2>Invoice {_selected.InvoiceNumber}</h2>");
        sb.Append($"<p><strong>Owner:</strong> {WebUtility.HtmlEncode(_selected.OwnerLabel)}<br><strong>Pet:</strong> {WebUtility.HtmlEncode(_selected.PetLabel)}<br><strong>Date:</strong> {_selected.Date:MMMM d, yyyy}</p>");
        sb.Append("<table><tr><th>Description</th><th class=\"r\">Qty</th><th class=\"r\">Rate</th><th class=\"r\">Amount</th></tr>");
        foreach (var l in _selected.Lines)
            sb.Append($"<tr><td>{WebUtility.HtmlEncode(l.Description)}</td><td class=\"r\">{l.Quantity:0.##}</td><td class=\"r\">{l.Rate:C}</td><td class=\"r\">{l.Amount:C}</td></tr>");
        sb.Append($"<tr class=\"tot\"><td colspan=\"3\" class=\"r\">Total</td><td class=\"r\">{_selected.Total:C}</td></tr>");
        sb.Append($"<tr><td colspan=\"3\" class=\"r\">Paid</td><td class=\"r\">{_selected.AmountPaid:C}</td></tr>");
        sb.Append($"<tr class=\"tot\"><td colspan=\"3\" class=\"r\">Balance due</td><td class=\"r\">{_selected.Balance:C}</td></tr>");
        sb.Append("</table><p>Thank you for trusting us with your pet.</p></body></html>");
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Invoice exported to {file.Path}";
    }
}
