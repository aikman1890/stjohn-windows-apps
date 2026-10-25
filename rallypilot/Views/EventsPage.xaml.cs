using System.Globalization;
using RallyPilot.Models;
using RallyPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RallyPilot.Views;

public sealed partial class EventsPage : Page
{
    private readonly JsonListStore<RallyEvent> _store = new("events.json");
    private readonly JsonListStore<AddOnSale> _saleStore = new("addonsales.json");
    private List<RallyEvent> _events = new();
    private RallyEvent? _selected;

    public EventsPage()
    {
        this.InitializeComponent();
        StatusBox.ItemsSource = new[] { "Planning", "Ready", "Completed", "Cancelled" };
        DatePicker.Date = DateTimeOffset.Now.AddDays(14);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _events = (await _store.LoadAsync()).OrderBy(ev => ev.EventDate).ToList();
        EventList.ItemsSource = _events;
        if (_events.Count > 0) EventList.SelectedIndex = 0;
    }

    private async Task RefreshLedgerAsync()
    {
        if (_selected is null) { LedgerText.Text = string.Empty; SaleList.ItemsSource = null; FollowText.Text = string.Empty; return; }
        var sales = await _saleStore.LoadAsync();
        var mine = sales.Where(s => s.EventId == _selected.Id).OrderBy(s => s.Date).ToList();
        SaleList.ItemsSource = mine;
        var final = _selected.QuotedTotal + mine.Sum(s => s.LineTotal);
        var collected = _selected.DepositPaid + _selected.PaymentsReceived;
        var balance = final - collected;
        var perHead = _selected.ConfirmedHeads > 0 ? final / _selected.ConfirmedHeads : 0m;
        LedgerText.Text = $"Final total {final:0.00} {_selected.Currency} (quoted {_selected.QuotedTotal:0.00} + add-ons {mine.Sum(s => s.LineTotal):0.00}) - collected {collected:0.00} - BALANCE {balance:0.00} - per-head realized {perHead:0.00}.";
        FollowText.Text = (_selected.ThankYouSent ? "Thank-you sent. " : "Thank-you not sent yet. ") +
            (_selected.RebookPromptDate is { } r ? $"Rebook prompt: {r:MMM d, yyyy}." : "No rebook prompt set.");
    }

    private async void EventList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = EventList.SelectedItem as RallyEvent;
        if (_selected is null) return;
        CompanyBox.Text = _selected.CompanyLabel;
        OrganizerBox.Text = string.IsNullOrWhiteSpace(_selected.OrganizerContact)
            ? _selected.OrganizerLabel : $"{_selected.OrganizerLabel} / {_selected.OrganizerContact}";
        DatePicker.Date = _selected.EventDate;
        HeadsBox.Text = $"{_selected.ExpectedHeads},{_selected.ConfirmedHeads}";
        QuotedBox.Text = _selected.QuotedTotal.ToString(CultureInfo.InvariantCulture);
        DepositBox.Text = $"{_selected.DepositDue.ToString(CultureInfo.InvariantCulture)},{_selected.DepositPaid.ToString(CultureInfo.InvariantCulture)}";
        PaymentsBox.Text = _selected.PaymentsReceived.ToString(CultureInfo.InvariantCulture);
        BalancePicker.Date = _selected.BalanceDueDate;
        ReqBox.Text = _selected.SpecialRequirements;
        StatusBox.SelectedItem = _selected.Status;
        await RefreshLedgerAsync();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CompanyBox.Text)) { ErrorText.Text = "Name the company."; return; }
        var ev = _selected ?? new RallyEvent();
        ev.CompanyLabel = CompanyBox.Text.Trim();
        var orgParts = OrganizerBox.Text.Split('/', 2);
        ev.OrganizerLabel = orgParts[0].Trim();
        if (orgParts.Length > 1) ev.OrganizerContact = orgParts[1].Trim();
        ev.EventDate = DatePicker.Date ?? ev.EventDate;
        var heads = HeadsBox.Text.Split(',');
        if (heads.Length == 2)
        {
            if (int.TryParse(heads[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var exp)) ev.ExpectedHeads = exp;
            if (int.TryParse(heads[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var conf)) ev.ConfirmedHeads = conf;
        }
        if (decimal.TryParse(QuotedBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var quoted)) ev.QuotedTotal = quoted;
        var dep = DepositBox.Text.Split(',');
        if (dep.Length == 2)
        {
            if (decimal.TryParse(dep[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dd)) ev.DepositDue = dd;
            if (decimal.TryParse(dep[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dp)) ev.DepositPaid = dp;
        }
        if (decimal.TryParse(PaymentsBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay)) ev.PaymentsReceived = pay;
        ev.BalanceDueDate = BalancePicker.Date;
        ev.SpecialRequirements = ReqBox.Text.Trim();
        ev.Status = StatusBox.SelectedItem as string ?? ev.Status;
        if (_selected is null) _events.Add(ev);
        await _store.SaveAsync(_events);
        _events = _events.OrderBy(x => x.EventDate).ToList();
        EventList.ItemsSource = _events;
        EventList.SelectedItem = _events.FirstOrDefault(x => x.Id == ev.Id);
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        EventList.SelectedItem = null;
        CompanyBox.Text = OrganizerBox.Text = ReqBox.Text = string.Empty;
        HeadsBox.Text = "0,0";
        QuotedBox.Text = "0";
        DepositBox.Text = "0,0";
        PaymentsBox.Text = "0";
        DatePicker.Date = DateTimeOffset.Now.AddDays(14);
        BalancePicker.Date = null;
        StatusBox.SelectedItem = "Planning";
        LedgerText.Text = FollowText.Text = string.Empty;
        SaleList.ItemsSource = null;
    }

    private async void ThanksButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the event."; return; }
        _selected.ThankYouSent = true;
        _selected.RebookPromptDate = DateTimeOffset.Now.AddMonths(11);
        await _store.SaveAsync(_events);
        await RefreshLedgerAsync();
    }

    private async void SaleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the event."; return; }
        if (string.IsNullOrWhiteSpace(SaleLabelBox.Text)) { ErrorText.Text = "Name the add-on."; return; }
        var sales = await _saleStore.LoadAsync();
        sales.Add(new AddOnSale
        {
            EventId = _selected.Id, EventLabel = _selected.CompanyLabel, Label = SaleLabelBox.Text.Trim(),
            Qty = decimal.TryParse(SaleQtyBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var qty) ? qty : 1m,
            UnitPrice = decimal.TryParse(SalePriceBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ? price : 0m,
            Date = DateTimeOffset.Now,
        });
        await _saleStore.SaveAsync(sales);
        SaleLabelBox.Text = SaleQtyBox.Text = SalePriceBox.Text = string.Empty;
        await RefreshLedgerAsync();
    }
}
