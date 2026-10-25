using System.Globalization;
using System.Net;
using System.Text;
using VenuePilot.Models;
using VenuePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace VenuePilot.Views;

public sealed partial class EventsPage : Page
{
    private readonly JsonListStore<VenueEvent> _store = new("events.json");
    private readonly JsonListStore<VenuePayment> _paymentStore = new("payments.json");
    private readonly JsonListStore<VendorEntry> _vendorStore = new("vendors.json");
    private readonly VenueSettingsStore _settingsStore = new();
    private List<VenueEvent> _items = new();
    private List<VenuePayment> _payments = new();
    private List<VendorEntry> _vendors = new();
    private List<string> _packages = new();
    private VenueEvent? _selected;

    public EventsPage()
    {
        this.InitializeComponent();
        DatePickerCtl.Date = DateTimeOffset.Now.AddDays(60);
        HoldPicker.Date = DateTimeOffset.Now.AddDays(14);
        BalancePicker.Date = DateTimeOffset.Now.AddDays(30);
        StatusCombo.SelectedIndex = 0;
        PayKindCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        SpacePicker.ItemsSource = settings.Spaces;
        TypeCombo.ItemsSource = settings.EventTypes;
        _packages = settings.Packages;
        PackagePicker.ItemsSource = _packages.Select(p => p.Split('|')[0]).ToList();
        if (settings.Spaces.Count > 0) SpacePicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        _payments = await _paymentStore.LoadAsync();
        _vendors = await _vendorStore.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(x => x.EventDate).ToList();
    }

    private void RefreshSubs()
    {
        if (_selected is null)
        {
            PaymentsList.ItemsSource = null;
            VendorsList.ItemsSource = null;
            MoneyText.Text = string.Empty;
            return;
        }
        var mine = _payments.Where(p => p.EventId == _selected.Id).OrderBy(p => p.Date).ToList();
        PaymentsList.ItemsSource = mine;
        VendorsList.ItemsSource = _vendors.Where(v => v.EventId == _selected.Id).ToList();
        var paid = mine.Where(p => p.Kind != "Refund").Sum(p => p.Amount);
        var refunds = mine.Where(p => p.Kind == "Refund").Sum(p => p.Amount);
        MoneyText.Text = $"Package {_selected.PackagePrice:C} - received {paid:C} - refunds {refunds:C} - BALANCE {_selected.PackagePrice - paid + refunds:C}";
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as VenueEvent;
        if (_selected is null) return;
        ClientBox.Text = _selected.ClientNames;
        PhoneBox.Text = _selected.Phone;
        EmailBox.Text = _selected.Email;
        SpacePicker.SelectedItem = _selected.Space;
        TypeCombo.Text = _selected.EventType;
        DatePickerCtl.Date = _selected.EventDate;
        StatusCombo.SelectedItem = _selected.Status;
        if (_selected.HoldExpiry is { } h) HoldPicker.Date = h;
        PackagePicker.SelectedItem = _selected.PackageName;
        PriceBox.Text = _selected.PackagePrice.ToString(CultureInfo.InvariantCulture);
        GuestsBox.Text = _selected.GuestCount.ToString();
        BalanceCheck.IsChecked = _selected.BalanceDueDate is not null;
        if (_selected.BalanceDueDate is { } b) BalancePicker.Date = b;
        SourceBox.Text = _selected.Source;
        LoadBox.Text = string.IsNullOrWhiteSpace(_selected.LoadOut)
            ? _selected.LoadIn : $"{_selected.LoadIn} / {_selected.LoadOut}";
        TimelineBox.Text = _selected.TimelineNotes;
        CancelBox.Text = _selected.CancelReason;
        NotesBox.Text = _selected.Notes;
        GuardText.Text = string.Empty;
        RefreshSubs();
    }

    private void BalanceCheck_Changed(object sender, RoutedEventArgs e)
    {
        BalancePicker.IsEnabled = BalanceCheck.IsChecked == true;
    }

    private void PackagePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PackagePicker.SelectedIndex < 0 || PackagePicker.SelectedIndex >= _packages.Count) return;
        var parts = _packages[PackagePicker.SelectedIndex].Split('|');
        if (parts.Length >= 2 && decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            PriceBox.Text = price.ToString(CultureInfo.InvariantCulture);
    }

    private decimal ParseMoney(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new VenueEvent();
        item.ClientNames = ClientBox.Text.Trim();
        item.Phone = PhoneBox.Text.Trim();
        item.Email = EmailBox.Text.Trim();
        item.Space = SpacePicker.SelectedItem as string ?? string.Empty;
        item.EventType = TypeCombo.Text.Trim();
        item.EventDate = DatePickerCtl.Date ?? DateTimeOffset.Now;
        item.Status = StatusCombo.SelectedItem as string ?? "Inquiry";
        item.HoldExpiry = item.Status == "Hold" ? HoldPicker.Date : null;
        item.PackageName = PackagePicker.SelectedItem as string ?? string.Empty;
        item.PackagePrice = ParseMoney(PriceBox.Text);
        item.GuestCount = int.TryParse(GuestsBox.Text, out var g) ? g : 0;
        if (item.Status == "Contracted" && _selected?.Status != "Contracted")
            item.ContractDate = DateTimeOffset.Now;
        item.BalanceDueDate = BalanceCheck.IsChecked == true ? BalancePicker.Date : null;
        item.Source = SourceBox.Text.Trim();
        var loadParts = LoadBox.Text.Split(new[] { '/' }, 2);
        item.LoadIn = loadParts[0].Trim();
        item.LoadOut = loadParts.Length > 1 ? loadParts[1].Trim() : string.Empty;
        item.TimelineNotes = TimelineBox.Text.Trim();
        item.CancelReason = CancelBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (item.Status == "Inquiry" && _selected is null) item.LastContacted = DateTimeOffset.Now;

        // HARD double-booking rule - a contracted date owns the space.
        var conflict = BookingGuard.ContractConflict(item, _items);
        if (conflict is not null)
        {
            GuardText.Text = $"NOT SAVED: {item.Space} is already contracted on {item.EventDate:MMM d, yyyy} to {conflict.ClientNames}. Two contracted events cannot share a space and date.";
            return;
        }
        var warnings = BookingGuard.Warnings(item, _items);
        GuardText.Text = warnings.Count > 0 ? string.Join(" ", warnings) : "Saved - no conflicts.";

        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        RefreshSubs();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(x => x.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
        RefreshSubs();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
        RefreshSubs();
    }

    private void ClearForm()
    {
        ClientBox.Text = string.Empty;
        PhoneBox.Text = string.Empty;
        EmailBox.Text = string.Empty;
        PriceBox.Text = string.Empty;
        GuestsBox.Text = string.Empty;
        SourceBox.Text = string.Empty;
        LoadBox.Text = string.Empty;
        TimelineBox.Text = string.Empty;
        CancelBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        GuardText.Text = string.Empty;
    }

    private async void PayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _payments.Add(new VenuePayment
        {
            EventId = _selected.Id,
            Date = DateTimeOffset.Now,
            Amount = ParseMoney(PayAmountBox.Text),
            Kind = PayKindCombo.SelectedItem as string ?? "Deposit",
        });
        await _paymentStore.SaveAsync(_payments);
        RefreshSubs();
    }

    private async void VendorButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _vendors.Add(new VendorEntry
        {
            EventId = _selected.Id,
            Role = VendorRoleBox.Text.Trim(),
            Name = VendorNameBox.Text.Trim(),
            Phone = VendorPhoneBox.Text.Trim(),
            ArrivalTime = VendorArrivalBox.Text.Trim(),
        });
        await _vendorStore.SaveAsync(_vendors);
        RefreshSubs();
    }

    private async void VendorDelButton_Click(object sender, RoutedEventArgs e)
    {
        if (VendorsList.SelectedItem is not VendorEntry v) return;
        _vendors.RemoveAll(x => x.Id == v.Id);
        await _vendorStore.SaveAsync(_vendors);
        RefreshSubs();
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void DaySheetButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        var settings = await _settingsStore.LoadAsync();
        var mine = _vendors.Where(v => v.EventId == _selected.Id).ToList();
        var pays = _payments.Where(p => p.EventId == _selected.Id).ToList();
        var paid = pays.Where(p => p.Kind != "Refund").Sum(p => p.Amount);
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Day-of sheet</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:36px;color:#222}h1{font-size:22px}h2{font-size:15px;margin-top:18px}table{border-collapse:collapse}td{padding:4px 14px 4px 0;font-size:14px}.muted{color:#666}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.VenueName)} - Day-of sheet</h1>");
        sb.AppendLine($"<p><strong>{Esc(_selected.ClientNames)}</strong> - {Esc(_selected.EventType)}<br/>" +
                      $"<strong>Date:</strong> {_selected.EventDate:dddd, MMM d, yyyy} - <strong>Space:</strong> {Esc(_selected.Space)} - <strong>Guests:</strong> {_selected.GuestCount}<br/>" +
                      $"<strong>Contact:</strong> {Esc(_selected.Phone)} {Esc(_selected.Email)}<br/>" +
                      $"<strong>Load-in:</strong> {Esc(_selected.LoadIn)} - <strong>Load-out:</strong> {Esc(_selected.LoadOut)}<br/>" +
                      $"<strong>Package:</strong> {Esc(_selected.PackageName)} {_selected.PackagePrice:C} - paid {paid:C} - balance {_selected.PackagePrice - paid:C}</p>");
        sb.AppendLine("<h2>Timeline</h2>");
        sb.AppendLine($"<p>{Esc(_selected.TimelineNotes)}</p>");
        sb.AppendLine("<h2>Vendors</h2><table>");
        foreach (var v in mine)
            sb.AppendLine($"<tr><td>{Esc(v.Role)}</td><td>{Esc(v.Name)}</td><td>{Esc(v.Phone)}</td><td>arrives {Esc(v.ArrivalTime)}</td></tr>");
        sb.AppendLine("</table>");
        sb.AppendLine($"<p class=\"muted\">Notes: {Esc(_selected.Notes)}</p>");
        sb.AppendLine("</body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = $"day-of-{_selected.EventDate:yyyy-MM-dd}-{_selected.Space}.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        GuardText.Text = $"Day-of sheet saved to {file.Path}";
    }
}
