using System.Globalization;
using System.Net;
using System.Text;
using MovePilot.Models;
using MovePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MovePilot.Views;

public sealed partial class MovesPage : Page
{
    private readonly JsonListStore<MoveJob> _store = new("moves.json");
    private readonly JsonListStore<InventoryItem> _inventoryStore = new("inventory.json");
    private readonly JsonListStore<MoveEstimate> _estimateStore = new("estimates.json");
    private readonly MoveSettingsStore _settingsStore = new();
    private List<MoveJob> _items = new();
    private List<InventoryItem> _inventory = new();
    private List<string> _presets = new();
    private MoveJob? _selected;

    public MovesPage()
    {
        this.InitializeComponent();
        MoveDatePicker.Date = DateTimeOffset.Now.AddDays(7);
        StatusCombo.SelectedIndex = 0;
        PackingCombo.SelectedIndex = 1;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        _presets = settings.ItemPresets;
        PresetPicker.ItemsSource = _presets.Select(p => p.Split('|')[0]).ToList();
        _items = await _store.LoadAsync();
        _inventory = await _inventoryStore.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(m => m.MoveDate).ToList();
    }

    private void RefreshInventory()
    {
        if (_selected is null)
        {
            InventoryList.ItemsSource = null;
            TotalsText.Text = "Select or save a move to build its inventory.";
            return;
        }
        var mine = _inventory.Where(i => i.MoveId == _selected.Id).ToList();
        InventoryList.ItemsSource = mine;
        TotalsText.Text = $"Totals: {mine.Sum(i => i.TotalCube):0} cu ft - {mine.Sum(i => i.TotalWeight):0} lb - {mine.Count} line(s), {mine.Sum(i => i.Qty)} piece(s)" +
                          (mine.Any(i => i.Special) ? $" - {mine.Count(i => i.Special)} special item line(s)" : string.Empty);
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as MoveJob;
        if (_selected is null) return;
        CustomerBox.Text = _selected.Customer;
        PhoneBox.Text = _selected.Phone;
        EmailBox.Text = _selected.Email;
        OriginBox.Text = _selected.Origin;
        DestBox.Text = _selected.Destination;
        MoveDatePicker.Date = _selected.MoveDate;
        StatusCombo.SelectedItem = _selected.Status;
        AccessBox.Text = _selected.AccessNotes;
        ValuationBox.Text = _selected.ValuationChoice;
        CrewBox.Text = _selected.CrewSize.ToString();
        TrucksBox.Text = _selected.Trucks.ToString();
        ContractBox.Text = _selected.ContractAmount.ToString(CultureInfo.InvariantCulture);
        DepositBox.Text = _selected.Deposit.ToString(CultureInfo.InvariantCulture);
        CollectedBox.Text = _selected.Collected.ToString(CultureInfo.InvariantCulture);
        NotesBox.Text = _selected.Notes;
        RefreshInventory();
    }

    private decimal ParseMoney(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new MoveJob();
        item.Customer = CustomerBox.Text.Trim();
        item.Phone = PhoneBox.Text.Trim();
        item.Email = EmailBox.Text.Trim();
        item.Origin = OriginBox.Text.Trim();
        item.Destination = DestBox.Text.Trim();
        item.MoveDate = MoveDatePicker.Date ?? DateTimeOffset.Now;
        item.Status = StatusCombo.SelectedItem as string ?? "Lead";
        item.AccessNotes = AccessBox.Text.Trim();
        item.ValuationChoice = ValuationBox.Text.Trim();
        item.CrewSize = int.TryParse(CrewBox.Text, out var c) ? c : 0;
        item.Trucks = int.TryParse(TrucksBox.Text, out var t) ? t : 0;
        item.ContractAmount = ParseMoney(ContractBox.Text);
        item.Deposit = ParseMoney(DepositBox.Text);
        item.Collected = ParseMoney(CollectedBox.Text);
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        RefreshInventory();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(m => m.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
        RefreshInventory();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
        RefreshInventory();
    }

    private void ClearForm()
    {
        CustomerBox.Text = string.Empty;
        PhoneBox.Text = string.Empty;
        EmailBox.Text = string.Empty;
        OriginBox.Text = string.Empty;
        DestBox.Text = string.Empty;
        AccessBox.Text = string.Empty;
        ValuationBox.Text = string.Empty;
        ContractBox.Text = string.Empty;
        DepositBox.Text = string.Empty;
        CollectedBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }

    private void PresetPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PresetPicker.SelectedIndex < 0 || PresetPicker.SelectedIndex >= _presets.Count) return;
        var parts = _presets[PresetPicker.SelectedIndex].Split('|');
        if (parts.Length >= 3)
        {
            ItemBox.Text = parts[0];
            CubeBox.Text = parts[1];
            WeightBox.Text = parts[2];
        }
    }

    private async void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            StatusText.Text = "Save/select a move first - inventory hangs off the move file.";
            return;
        }
        _inventory.Add(new InventoryItem
        {
            MoveId = _selected.Id,
            Room = RoomBox.Text.Trim(),
            ItemName = ItemBox.Text.Trim(),
            Qty = int.TryParse(QtyBox.Text, out var q) ? q : 1,
            CubePerItem = ParseMoney(CubeBox.Text),
            WeightPerItem = ParseMoney(WeightBox.Text),
            Packing = PackingCombo.SelectedItem as string ?? "Owner-packed",
            Special = SpecialCheck.IsChecked == true,
        });
        await _inventoryStore.SaveAsync(_inventory);
        RefreshInventory();
    }

    private async void RemoveItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (InventoryList.SelectedItem is not InventoryItem line) return;
        _inventory.RemoveAll(i => i.Id == line.Id);
        await _inventoryStore.SaveAsync(_inventory);
        RefreshInventory();
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async Task<string?> PickHtmlAsync(string suggested)
    {
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = suggested;
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        return file?.Path is null ? null : file.Path;
    }

    private async void SheetButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { StatusText.Text = "Select a move first."; return; }
        var settings = await _settingsStore.LoadAsync();
        var mine = _inventory.Where(i => i.MoveId == _selected.Id).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Job sheet</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:22px}h2{font-size:16px;margin-top:20px}table{border-collapse:collapse;width:100%}td,th{border:1px solid #bbb;padding:6px 8px;text-align:left;font-size:13px}.muted{color:#666}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.CompanyName)} - Crew job sheet</h1>");
        sb.AppendLine($"<p><strong>Customer:</strong> {Esc(_selected.Customer)} ({Esc(_selected.Phone)})<br/>" +
                      $"<strong>Move date:</strong> {_selected.MoveDate:dddd, MMM d, yyyy}<br/>" +
                      $"<strong>From:</strong> {Esc(_selected.Origin)}<br/><strong>To:</strong> {Esc(_selected.Destination)}<br/>" +
                      $"<strong>Crew:</strong> {_selected.CrewSize} - <strong>Trucks:</strong> {_selected.Trucks}<br/>" +
                      $"<strong>Contract:</strong> {_selected.ContractAmount:C} - deposit paid {_selected.Deposit:C} - <strong>collect on delivery: {_selected.Balance:C}</strong></p>");
        sb.AppendLine($"<h2>Access notes</h2><p>{Esc(_selected.AccessNotes)}</p>");
        sb.AppendLine("<h2>Inventory</h2><table><tr><th>Room</th><th>Item</th><th>Qty</th><th>Cu ft</th><th>Lb</th><th>Packing</th></tr>");
        foreach (var i in mine)
            sb.AppendLine($"<tr><td>{Esc(i.Room)}</td><td>{Esc(i.ItemName)}{(i.Special ? " (SPECIAL)" : string.Empty)}</td><td>{i.Qty}</td><td>{i.TotalCube:0}</td><td>{i.TotalWeight:0}</td><td>{Esc(i.Packing)}</td></tr>");
        sb.AppendLine($"</table><p><strong>Totals: {mine.Sum(i => i.TotalCube):0} cu ft, {mine.Sum(i => i.TotalWeight):0} lb</strong></p>");
        sb.AppendLine($"<p class=\"muted\">Valuation on file: {Esc(_selected.ValuationChoice)}</p>");
        sb.AppendLine("</body></html>");
        var path = await PickHtmlAsync($"job-sheet-{_selected.Customer.Replace(" ", "-")}.html");
        if (path is null) return;
        await File.WriteAllTextAsync(path, sb.ToString());
        StatusText.Text = $"Job sheet saved to {path}";
    }

    private async void StatementButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { StatusText.Text = "Select a move first."; return; }
        var settings = await _settingsStore.LoadAsync();
        var estimates = (await _estimateStore.LoadAsync()).Where(x => x.MoveId == _selected.Id).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Final statement</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:22px}table{border-collapse:collapse}td{padding:4px 12px 4px 0;font-size:14px}.muted{color:#666}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.CompanyName)} - Final statement</h1>");
        sb.AppendLine($"<p><strong>Customer:</strong> {Esc(_selected.Customer)}<br/><strong>Move:</strong> {Esc(_selected.Origin)} -&gt; {Esc(_selected.Destination)} on {_selected.MoveDate:MMM d, yyyy}<br/><strong>Status:</strong> {Esc(_selected.Status)}</p>");
        sb.AppendLine("<table>");
        sb.AppendLine($"<tr><td>Contract amount</td><td>{_selected.ContractAmount:C}</td></tr>");
        sb.AppendLine($"<tr><td>Deposit received</td><td>-{_selected.Deposit:C}</td></tr>");
        sb.AppendLine($"<tr><td>Collected</td><td>-{_selected.Collected:C}</td></tr>");
        sb.AppendLine($"<tr><td><strong>Balance</strong></td><td><strong>{_selected.Balance:C}</strong></td></tr>");
        sb.AppendLine("</table>");
        var approved = estimates.FirstOrDefault(x => x.Status == "Approved");
        if (approved is not null)
            sb.AppendLine($"<p class=\"muted\">Per approved estimate v{approved.Version} ({Esc(approved.Type)}, {approved.Amount:C}), approved by {Esc(approved.DecidedBy)} via {Esc(approved.DecisionMethod)} on {approved.DecisionDate:MMM d, yyyy}.</p>");
        sb.AppendLine("</body></html>");
        var path = await PickHtmlAsync($"final-statement-{_selected.Customer.Replace(" ", "-")}.html");
        if (path is null) return;
        await File.WriteAllTextAsync(path, sb.ToString());
        StatusText.Text = $"Final statement saved to {path}";
    }
}
