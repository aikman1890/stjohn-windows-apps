using LienPilot.Models;
using LienPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LienPilot.Views;

public sealed partial class UnitsPage : Page
{
    private readonly JsonListStore<StorageUnit> _store = new("units.json");
    private readonly JsonListStore<LienLedgerEntry> _ledgerStore = new("ledger.json");
    private readonly JsonListStore<SaleRecord> _saleStore = new("sales.json");
    private List<StorageUnit> _items = new();
    private StorageUnit? _selected;

    public UnitsPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = new[]
        {
            "Current", "Late", "Lien pending", "Notice sent", "Advertised",
            "Sale scheduled", "Released", "Sold", "Vacated",
        };
        StatusCombo.SelectedIndex = 0;
        PaidThroughPicker.Date = DateTimeOffset.Now;
        DefaultPicker.Date = DateTimeOffset.Now;
        SalePicker.Date = DateTimeOffset.Now.AddDays(14);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(u => u.UnitNumber).ToList();
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private async void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as StorageUnit;
        if (_selected is null) return;
        UnitBox.Text = _selected.UnitNumber;
        TenantBox.Text = _selected.TenantName;
        PhoneBox.Text = _selected.TenantPhone;
        EmailBox.Text = _selected.TenantEmail;
        AddressBox.Text = _selected.TenantAddress;
        RateBox.Text = _selected.MonthlyRate.ToString("0.##");
        BalanceBox.Text = _selected.Balance.ToString("0.##");
        if (_selected.PaidThrough is { } pt) PaidThroughPicker.Date = pt;
        StatusCombo.SelectedItem = _selected.Status;
        DefaultCheck.IsChecked = _selected.DefaultDate is not null;
        if (_selected.DefaultDate is { } dd) DefaultPicker.Date = dd;
        SaleCheck.IsChecked = _selected.SaleDate is not null;
        if (_selected.SaleDate is { } sd) SalePicker.Date = sd;
        BankruptcyCheck.IsChecked = _selected.BankruptcyFlag;
        MilitaryCheck.IsChecked = _selected.MilitaryFlag;
        DisputeCheck.IsChecked = _selected.DisputeFlag;
        NotesBox.Text = _selected.Notes;

        var ledger = (await _ledgerStore.LoadAsync()).Where(l => l.UnitId == _selected.Id).OrderBy(l => l.Date).ToList();
        var sales = (await _saleStore.LoadAsync()).Where(s => s.UnitId == _selected.Id).ToList();
        var lines = new List<string> { $"History for unit {_selected.UnitNumber}:" };
        lines.AddRange(ledger.Select(l => $"  {l.Date:MMM d, yyyy} - {l.StepType} ({l.Method})"));
        lines.AddRange(sales.Select(s => $"  {s.Date:MMM d, yyyy} - SOLD {s.SalePrice:C}, applied {s.AppliedToClaim:C}, excess {s.Excess:C}"));
        if (ledger.Count == 0 && sales.Count == 0) lines.Add("  No notices or sales recorded yet.");
        HistoryText.Text = string.Join(Environment.NewLine, lines);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new StorageUnit();
        item.UnitNumber = UnitBox.Text.Trim();
        item.TenantName = TenantBox.Text.Trim();
        item.TenantPhone = PhoneBox.Text.Trim();
        item.TenantEmail = EmailBox.Text.Trim();
        item.TenantAddress = AddressBox.Text.Trim();
        item.MonthlyRate = ParseDec(RateBox.Text);
        item.Balance = ParseDec(BalanceBox.Text);
        item.PaidThrough = PaidThroughPicker.Date;
        item.Status = StatusCombo.SelectedItem as string ?? "Current";
        item.DefaultDate = DefaultCheck.IsChecked == true ? DefaultPicker.Date : null;
        item.SaleDate = SaleCheck.IsChecked == true ? SalePicker.Date : null;
        item.BankruptcyFlag = BankruptcyCheck.IsChecked == true;
        item.MilitaryFlag = MilitaryCheck.IsChecked == true;
        item.DisputeFlag = DisputeCheck.IsChecked == true;
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(u => u.Id == _selected.Id);
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
        UnitBox.Text = string.Empty;
        TenantBox.Text = string.Empty;
        PhoneBox.Text = string.Empty;
        EmailBox.Text = string.Empty;
        AddressBox.Text = string.Empty;
        RateBox.Text = string.Empty;
        BalanceBox.Text = string.Empty;
        StatusCombo.SelectedIndex = 0;
        DefaultCheck.IsChecked = false;
        SaleCheck.IsChecked = false;
        BankruptcyCheck.IsChecked = false;
        MilitaryCheck.IsChecked = false;
        DisputeCheck.IsChecked = false;
        NotesBox.Text = string.Empty;
        HistoryText.Text = string.Empty;
    }
}
