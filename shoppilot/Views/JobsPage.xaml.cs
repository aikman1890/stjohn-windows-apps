using System.Globalization;
using System.Net;
using System.Text;
using ShopPilot.Models;
using ShopPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ShopPilot.Views;

public sealed partial class JobsPage : Page
{
    private readonly JsonListStore<ShopJob> _store = new("jobs.json");
    private readonly JsonListStore<ShopCustomer> _customerStore = new("customers.json");
    private readonly JsonListStore<JobOperation> _opStore = new("operations.json");
    private readonly JsonListStore<OutsidePO> _poStore = new("outsidepos.json");
    private readonly ShopSettingsStore _settingsStore = new();
    private List<ShopJob> _items = new();
    private List<ShopCustomer> _customers = new();
    private List<JobOperation> _ops = new();
    private List<string> _library = new();
    private ShopJob? _selected;
    private JobOperation? _selectedOp;
    private decimal _laborRate = 85m;

    public JobsPage()
    {
        this.InitializeComponent();
        DuePicker.Date = DateTimeOffset.Now.AddDays(7);
        StatusCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        _laborRate = settings.LaborRate;
        _library = settings.OperationLibrary;
        OpPicker.ItemsSource = _library;
        CenterPicker.ItemsSource = settings.WorkCenters;
        _customers = await _customerStore.LoadAsync();
        CustomerPicker.ItemsSource = _customers.Select(c => c.Name).ToList();
        if (_customers.Count > 0) CustomerPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        _ops = await _opStore.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(j => j.DueDate).ToList();
    }

    private async void RefreshOps()
    {
        if (_selected is null)
        {
            OpsList.ItemsSource = null;
            CostText.Text = string.Empty;
            return;
        }
        var mine = _ops.Where(o => o.JobId == _selected.Id).OrderBy(o => o.Seq).ToList();
        OpsList.ItemsSource = mine;
        var pos = await _poStore.LoadAsync();
        var laborHours = mine.Sum(o => o.ActualHours);
        var laborCost = laborHours * _laborRate;
        var outside = pos.Where(p => p.JobId == _selected.Id).Sum(p => p.Cost);
        var actual = _selected.MaterialCost + laborCost + outside;
        var variance = _selected.QuotedPrice - actual;
        CostText.Text =
            $"Costing: quoted {_selected.QuotedPrice:C} - material {_selected.MaterialCost:C} - labor {laborHours:0.0}h x {_laborRate:C} = {laborCost:C} - outside {outside:C}" +
            $"{Environment.NewLine}Actual cost {actual:C} - margin {variance:C} ({(variance >= 0 ? "ahead" : "BEHIND")})";
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as ShopJob;
        if (_selected is null) return;
        var idx = _customers.FindIndex(c => c.Id == _selected.CustomerId);
        if (idx >= 0) CustomerPicker.SelectedIndex = idx;
        PartBox.Text = _selected.PartDescription;
        PartNoBox.Text = _selected.PartNumber;
        QtyBox.Text = _selected.Quantity.ToString();
        ShipQtyBox.Text = _selected.QtyShipped.ToString();
        MaterialBox.Text = _selected.Material;
        CertBox.Text = _selected.MaterialCertRef;
        MatCostBox.Text = _selected.MaterialCost.ToString(CultureInfo.InvariantCulture);
        PriceBox.Text = _selected.QuotedPrice.ToString(CultureInfo.InvariantCulture);
        DuePicker.Date = _selected.DueDate;
        PoBox.Text = _selected.CustomerPO;
        PriorityBox.Text = _selected.Priority.ToString();
        CenterPicker.SelectedItem = _selected.WorkCenter;
        StatusCombo.SelectedItem = _selected.Status;
        InvoiceBox.Text = _selected.InvoiceRef;
        NotesBox.Text = _selected.Notes;
        _selectedOp = null;
        RefreshOps();
    }

    private decimal ParseMoney(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerPicker.SelectedIndex < 0 || CustomerPicker.SelectedIndex >= _customers.Count) return;
        var customer = _customers[CustomerPicker.SelectedIndex];
        var item = _selected ?? new ShopJob { JobNumber = "J-" + (2000 + _items.Count + 1).ToString() };
        item.CustomerId = customer.Id;
        item.CustomerLabel = customer.Name;
        item.PartDescription = PartBox.Text.Trim();
        item.PartNumber = PartNoBox.Text.Trim();
        item.Quantity = int.TryParse(QtyBox.Text, out var q) ? q : 1;
        item.QtyShipped = int.TryParse(ShipQtyBox.Text, out var sq) ? sq : 0;
        item.Material = MaterialBox.Text.Trim();
        item.MaterialCertRef = CertBox.Text.Trim();
        item.MaterialCost = ParseMoney(MatCostBox.Text);
        item.QuotedPrice = ParseMoney(PriceBox.Text);
        item.DueDate = DuePicker.Date;
        item.CustomerPO = PoBox.Text.Trim();
        item.Priority = int.TryParse(PriorityBox.Text, out var p) ? p : 5;
        item.WorkCenter = CenterPicker.SelectedItem as string ?? string.Empty;
        item.Status = StatusCombo.SelectedItem as string ?? "Open";
        item.InvoiceRef = InvoiceBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        RefreshOps();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(j => j.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
        RefreshOps();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
        RefreshOps();
    }

    private void ClearForm()
    {
        PartBox.Text = string.Empty;
        PartNoBox.Text = string.Empty;
        QtyBox.Text = string.Empty;
        ShipQtyBox.Text = string.Empty;
        MaterialBox.Text = string.Empty;
        CertBox.Text = string.Empty;
        MatCostBox.Text = string.Empty;
        PriceBox.Text = string.Empty;
        PoBox.Text = string.Empty;
        PriorityBox.Text = string.Empty;
        InvoiceBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        StatusText.Text = string.Empty;
    }

    private async void ShipButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.ShipDate = DateTimeOffset.Now;
        _selected.QtyShipped = _selected.Quantity;
        ShipQtyBox.Text = _selected.QtyShipped.ToString();
        _selected.Status = "Shipped";
        await _store.SaveAsync(_items);
        RefreshList();
    }

    private async void AddOpButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { StatusText.Text = "Save/select a job first."; return; }
        if (OpPicker.SelectedItem is not string entry) return;
        var parts = entry.Split('|');
        var mine = _ops.Where(o => o.JobId == _selected.Id).ToList();
        _ops.Add(new JobOperation
        {
            JobId = _selected.Id,
            Seq = mine.Count + 1,
            Name = parts[0].Trim(),
            WorkCenter = parts.Length > 1 ? parts[1].Trim() : string.Empty,
            PlannedHours = ParseMoney(PlannedBox.Text),
        });
        await _opStore.SaveAsync(_ops);
        RefreshOps();
    }

    private void OpsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedOp = OpsList.SelectedItem as JobOperation;
        if (_selectedOp is null) return;
        ActualBox.Text = _selectedOp.ActualHours.ToString(CultureInfo.InvariantCulture);
        SignBox.Text = _selectedOp.SignOff;
    }

    private async void SaveOpButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedOp is null) return;
        _selectedOp.ActualHours = ParseMoney(ActualBox.Text);
        _selectedOp.SignOff = SignBox.Text.Trim();
        _selectedOp.Done = !string.IsNullOrWhiteSpace(_selectedOp.SignOff);
        if (_selectedOp.Done && _selectedOp.SignDate is null) _selectedOp.SignDate = DateTimeOffset.Now;
        await _opStore.SaveAsync(_ops);
        RefreshOps();
    }

    private async void RemoveOpButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedOp is null || _selected is null) return;
        _ops.RemoveAll(o => o.Id == _selectedOp.Id);
        var seq = 1;
        foreach (var op in _ops.Where(o => o.JobId == _selected.Id).OrderBy(o => o.Seq)) op.Seq = seq++;
        await _opStore.SaveAsync(_ops);
        _selectedOp = null;
        RefreshOps();
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void TravelerButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { StatusText.Text = "Select a job first."; return; }
        var settings = await _settingsStore.LoadAsync();
        var mine = _ops.Where(o => o.JobId == _selected.Id).OrderBy(o => o.Seq).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Traveler</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:32px;color:#222}h1{font-size:22px}table{border-collapse:collapse;width:100%}td,th{border:1px solid #999;padding:7px 8px;text-align:left;font-size:13px}.sign{width:150px}.muted{color:#666}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.ShopName)} - TRAVELER {_selected.JobNumber}</h1>");
        sb.AppendLine($"<p><strong>Customer:</strong> {Esc(_selected.CustomerLabel)} - PO {Esc(_selected.CustomerPO)}<br/>" +
                      $"<strong>Part:</strong> {Esc(_selected.PartDescription)} ({Esc(_selected.PartNumber)})<br/>" +
                      $"<strong>Material:</strong> {Esc(_selected.Material)} - cert: {Esc(_selected.MaterialCertRef)}<br/>" +
                      $"<strong>Quantity:</strong> {_selected.Quantity} - <strong>Due:</strong> {_selected.DueDate:MMM d, yyyy}</p>");
        sb.AppendLine("<table><tr><th>#</th><th>Operation</th><th>Work center</th><th>Planned hrs</th><th>Actual hrs</th><th>Sign-off</th><th>Date</th></tr>");
        foreach (var op in mine)
            sb.AppendLine($"<tr><td>{op.Seq}</td><td>{Esc(op.Name)}</td><td>{Esc(op.WorkCenter)}</td><td>{op.PlannedHours:0.0}</td><td></td><td class=\"sign\"></td><td class=\"sign\"></td></tr>");
        sb.AppendLine("</table>");
        sb.AppendLine($"<p>{Esc(_selected.Notes)}</p>");
        sb.AppendLine("<p class=\"muted\">Sign each operation as it completes and note actual hours - the office enters them in ShopPilot for job costing.</p>");
        sb.AppendLine("</body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = $"traveler-{_selected.JobNumber}.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Traveler saved to {file.Path} - open in a browser and print for the floor.";
    }
}
