using System.Net;
using System.Text;
using InvoicePilot.Models;
using InvoicePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace InvoicePilot.Views;

public sealed partial class InvoicesPage : Page
{
    private static readonly string[] Statuses =
    {
        "Draft",
        "Issued",
        "Paid",
        "Overdue",
        "Void",
    };

    private readonly JsonListStore<Invoice> _store = new("invoices.json");
    private List<Invoice> _items = new();
    private Invoice? _selected;
    private List<InvoiceLineItem> _editItems = new();

    public InvoicesPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = Statuses;
        StatusCombo.SelectedIndex = 0;
        IssuePicker.Date = DateTimeOffset.Now;
        DuePicker.Date = DateTimeOffset.Now.AddDays(14);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
        if (NumberBox.Text.Length == 0) NumberBox.Text = SuggestNumber();
        RefreshEditItems();
    }

    private string SuggestNumber() => $"BCCS-{DateTime.Now:yyyy}-{_items.Count + 1:D3}";

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(i => i.IssueDate).ToList();
    }

    private void RefreshEditItems()
    {
        if (ItemsEditList is null || TotalsText is null) return;
        ItemsEditList.ItemsSource = null;
        ItemsEditList.ItemsSource = _editItems;
        var subtotal = _editItems.Sum(i => i.Amount);
        var taxPct = ParseDouble(TaxBox.Text);
        var tax = subtotal * taxPct / 100.0;
        TotalsText.Text = $"Subtotal {subtotal:C}  -  Tax {tax:C}  -  Total {subtotal + tax:C}";
    }

    private static double ParseDouble(string text) =>
        double.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0;

    private void TaxBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshEditItems();

    private void RescuePresetButton_Click(object sender, RoutedEventArgs e)
    {
        _editItems.Add(new InvoiceLineItem
        {
            Description = "Emergency Linux & Server Rescue - first 4 hours",
            Quantity = 1,
            Rate = 500,
        });
        RefreshEditItems();
    }

    private void ConsultPresetButton_Click(object sender, RoutedEventArgs e)
    {
        _editItems.Add(new InvoiceLineItem
        {
            Description = "SRE / Linux consulting (per hour)",
            Quantity = 1,
            Rate = 65,
        });
        RefreshEditItems();
    }

    private void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ItemDescBox.Text)) return;
        _editItems.Add(new InvoiceLineItem
        {
            Description = ItemDescBox.Text.Trim(),
            Quantity = ParseDouble(ItemQtyBox.Text) is var q && q > 0 ? q : 1,
            Rate = ParseDouble(ItemRateBox.Text),
        });
        ItemDescBox.Text = string.Empty;
        ItemQtyBox.Text = "1";
        ItemRateBox.Text = "0";
        RefreshEditItems();
    }

    private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsEditList.SelectedItem is InvoiceLineItem li)
        {
            _editItems.Remove(li);
            RefreshEditItems();
        }
    }

    private void ItemsEditList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemsEditList.SelectedItem is InvoiceLineItem li)
        {
            ItemDescBox.Text = li.Description;
            ItemQtyBox.Text = li.Quantity.ToString("0.##");
            ItemRateBox.Text = li.Rate.ToString("0.##");
        }
    }

    private Invoice BuildFromForm(Invoice target)
    {
        target.InvoiceNumber = NumberBox.Text.Trim();
        target.ClientName = ClientBox.Text.Trim();
        target.ClientContact = ContactBox.Text.Trim();
        target.IssueDate = IssuePicker.Date;
        target.DueDate = DueCheck.IsChecked == true ? DuePicker.Date : null;
        target.Status = StatusCombo.SelectedItem as string ?? Statuses[0];
        target.TaxPercent = ParseDouble(TaxBox.Text);
        target.Notes = NotesBox.Text.Trim();
        target.LineItems = _editItems;
        return target;
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Invoice;
        if (_selected is null) return;
        NumberBox.Text = _selected.InvoiceNumber;
        ClientBox.Text = _selected.ClientName;
        ContactBox.Text = _selected.ClientContact;
        IssuePicker.Date = _selected.IssueDate;
        DueCheck.IsChecked = _selected.DueDate is not null;
        if (_selected.DueDate is { } d) DuePicker.Date = d;
        StatusCombo.SelectedItem = _selected.Status;
        TaxBox.Text = _selected.TaxPercent.ToString("0.##");
        NotesBox.Text = _selected.Notes;
        _editItems = new List<InvoiceLineItem>(_selected.LineItems);
        RefreshEditItems();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ClientBox.Text)) return;
        var item = BuildFromForm(_selected ?? new Invoice());
        if (string.IsNullOrWhiteSpace(item.InvoiceNumber)) item.InvoiceNumber = SuggestNumber();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        _editItems = item.LineItems;
        RefreshList();
        RefreshEditItems();
        ItemsList.SelectedItem = _items.FirstOrDefault(i => i.Id == item.Id);
        StatusText.Text = $"Saved invoice {item.InvoiceNumber}.";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(i => i.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
    }

    private void ClearForm()
    {
        NumberBox.Text = SuggestNumber();
        ClientBox.Text = string.Empty;
        ContactBox.Text = string.Empty;
        IssuePicker.Date = DateTimeOffset.Now;
        DueCheck.IsChecked = false;
        DuePicker.Date = DateTimeOffset.Now.AddDays(14);
        StatusCombo.SelectedIndex = 0;
        TaxBox.Text = "0";
        NotesBox.Text = string.Empty;
        _editItems = new List<InvoiceLineItem>();
        RefreshEditItems();
        StatusText.Text = string.Empty;
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var invoice = BuildFromForm(new Invoice());
        if (string.IsNullOrWhiteSpace(invoice.ClientName))
        {
            StatusText.Text = "Enter a client name before exporting.";
            return;
        }
        if (App.WindowInstance is null) return;

        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = string.IsNullOrWhiteSpace(invoice.InvoiceNumber) ? "invoice" : invoice.InvoiceNumber,
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildHtml(invoice));
        StatusText.Text = $"Exported to {file.Path} - open it in a browser to print or save as PDF.";
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string BuildHtml(Invoice inv)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Invoice {Enc(inv.InvoiceNumber)}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "h1{margin-bottom:0}table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}" +
                  "th{background:#f0f0f0}.r{text-align:right}.tot{font-weight:bold}</style></head><body>");
        sb.Append("<h1>Bayou City Computer Solutions LLC</h1>");
        sb.Append("<p>Conroe, Texas &middot; (832) 906-0083<br>Business hours: 6:00 AM - 6:00 PM CT, 7 days a week</p>");
        sb.Append($"<h2>Invoice {Enc(inv.InvoiceNumber)}</h2>");
        sb.Append($"<p><strong>Client:</strong> {Enc(inv.ClientName)}<br>");
        if (!string.IsNullOrWhiteSpace(inv.ClientContact))
            sb.Append($"<strong>Contact:</strong> {Enc(inv.ClientContact)}<br>");
        sb.Append($"<strong>Issued:</strong> {inv.IssueDate:MMMM d, yyyy}<br>");
        if (inv.DueDate is { } due)
            sb.Append($"<strong>Due:</strong> {due:MMMM d, yyyy}<br>");
        sb.Append($"<strong>Status:</strong> {Enc(inv.Status)}</p>");
        sb.Append("<table><tr><th>Description</th><th class=\"r\">Qty</th><th class=\"r\">Rate</th><th class=\"r\">Amount</th></tr>");
        foreach (var li in inv.LineItems)
        {
            sb.Append($"<tr><td>{Enc(li.Description)}</td><td class=\"r\">{li.Quantity:0.##}</td>" +
                      $"<td class=\"r\">{li.Rate:C}</td><td class=\"r\">{li.Amount:C}</td></tr>");
        }
        sb.Append($"<tr><td colspan=\"3\" class=\"r\">Subtotal</td><td class=\"r\">{inv.Subtotal:C}</td></tr>");
        if (inv.TaxPercent > 0)
            sb.Append($"<tr><td colspan=\"3\" class=\"r\">Tax ({inv.TaxPercent:0.##}%)</td><td class=\"r\">{inv.TaxAmount:C}</td></tr>");
        sb.Append($"<tr class=\"tot\"><td colspan=\"3\" class=\"r\">Total due</td><td class=\"r\">{inv.Total:C}</td></tr>");
        sb.Append("</table>");
        if (!string.IsNullOrWhiteSpace(inv.Notes))
            sb.Append($"<p><strong>Notes:</strong> {Enc(inv.Notes)}</p>");
        sb.Append("<p>Thank you for your business.</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
