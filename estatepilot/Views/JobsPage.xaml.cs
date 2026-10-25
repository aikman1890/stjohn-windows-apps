using System.Globalization;
using System.Net;
using System.Text;
using EstatePilot.Models;
using EstatePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace EstatePilot.Views;

public sealed partial class JobsPage : Page
{
    private readonly JsonListStore<JobEntry> _store = new("jobs.json");
    private readonly JsonListStore<Property> _propertyStore = new("properties.json");
    private List<JobEntry> _jobs = new();
    private List<Property> _properties = new();
    private JobEntry? _selected;

    public JobsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _properties = (await _propertyStore.LoadAsync()).OrderBy(p => p.Name).ToList();
        PropertyBox.ItemsSource = _properties.Select(p => p.Name).ToList();
        _jobs = (await _store.LoadAsync()).OrderByDescending(j => j.Date).ToList();
        JobList.ItemsSource = _jobs;
        if (_jobs.Count > 0) JobList.SelectedIndex = 0;
    }

    private void JobList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = JobList.SelectedItem as JobEntry;
        if (_selected is null) return;
        PropertyBox.SelectedItem = _selected.PropertyLabel;
        VendorBox.Text = _selected.VendorLabel;
        WorkBox.Text = _selected.Work;
        DatePicker.Date = _selected.Date;
        CostBox.Text = _selected.Cost.ToString(CultureInfo.InvariantCulture);
        InvoiceBox.Text = _selected.InvoiceRef;
        WarrantyPicker.Date = _selected.WarrantyExpiry;
        CurrencyBox.Text = _selected.Currency;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (PropertyBox.SelectedIndex < 0) { ErrorText.Text = "Pick the property."; return; }
        if (string.IsNullOrWhiteSpace(WorkBox.Text)) { ErrorText.Text = "Describe the work."; return; }
        var j = _selected ?? new JobEntry();
        var p = _properties[PropertyBox.SelectedIndex];
        j.PropertyId = p.Id; j.PropertyLabel = p.Name;
        j.VendorLabel = VendorBox.Text.Trim();
        j.Work = WorkBox.Text.Trim();
        j.Date = DatePicker.Date ?? j.Date;
        if (decimal.TryParse(CostBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var cost)) j.Cost = cost;
        j.InvoiceRef = InvoiceBox.Text.Trim();
        j.WarrantyExpiry = WarrantyPicker.Date;
        j.Currency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        if (_selected is null) _jobs.Add(j);
        await _store.SaveAsync(_jobs);
        _jobs = _jobs.OrderByDescending(x => x.Date).ToList();
        JobList.ItemsSource = _jobs;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        JobList.SelectedItem = null;
        VendorBox.Text = WorkBox.Text = CostBox.Text = InvoiceBox.Text = string.Empty;
        CurrencyBox.Text = "USD";
        DatePicker.Date = DateTimeOffset.Now;
        WarrantyPicker.Date = null;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _jobs.Remove(_selected);
        await _store.SaveAsync(_jobs);
        _selected = null;
        JobList.ItemsSource = _jobs;
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (PropertyBox.SelectedIndex < 0) { ErrorText.Text = "Pick the property to export."; return; }
        var p = _properties[PropertyBox.SelectedIndex];
        var rows = _jobs.Where(j => j.PropertyId == p.Id).OrderBy(j => j.Date).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Work ledger</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:20px}li{margin:3px 0}</style></head><body>");
        sb.AppendLine($"<h1>Work ledger - {Esc(p.Name)}</h1><ul>");
        foreach (var j in rows)
            sb.AppendLine($"<li>{j.Date:MMM d, yyyy} - {Esc(j.Work)} - {Esc(j.VendorLabel)} - {j.Cost:0.00} {Esc(j.Currency)} (inv. {Esc(j.InvoiceRef)}){(j.WarrantyExpiry is { } we ? $" - warranty to {we:MMM d, yyyy}" : string.Empty)}</li>");
        sb.AppendLine("</ul>");
        foreach (var g in rows.GroupBy(j => j.Currency))
            sb.AppendLine($"<p><strong>Total recorded ({Esc(g.Key)}): {g.Sum(j => j.Cost):0.00}</strong></p>");
        sb.AppendLine("<p>Exported from EstatePilot.</p></body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = "work-ledger.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Ledger saved to {file.Path}";
    }
}
