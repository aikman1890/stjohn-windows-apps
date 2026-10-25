using FarewellPilot.Models;
using FarewellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FarewellPilot.Views;

public sealed partial class PreneedPage : Page
{
    private readonly JsonListStore<PreneedContract> _store = new("preneed.json");
    private readonly JsonListStore<DepositEntry> _depositStore = new("deposits.json");
    private List<PreneedContract> _items = new();
    private List<DepositEntry> _deposits = new();
    private PreneedContract? _selected;

    public PreneedPage()
    {
        this.InitializeComponent();
        FundingCombo.ItemsSource = new[] { "Trust", "Insurance" };
        FundingCombo.SelectedIndex = 0;
        StatusCombo.ItemsSource = new[] { "Active", "Fulfilled", "Transferred", "Cancelled" };
        StatusCombo.SelectedIndex = 0;
        DatePicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        _deposits = await _depositStore.LoadAsync();
        RefreshAll();
    }

    private void RefreshAll()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(p => p.Date).ToList();
        DepositsList.ItemsSource = _selected is null
            ? null
            : _deposits.Where(d => d.PreneedId == _selected.Id).OrderByDescending(d => d.Date).ToList();
        DepositSummaryText.Text = _selected is null
            ? string.Empty
            : $"{_selected.Number}: contracted {_selected.TotalAmount:C} - deposited {_deposits.Where(d => d.PreneedId == _selected.Id).Sum(d => d.Amount):C} - undeposited {_selected.TotalAmount - _deposits.Where(d => d.PreneedId == _selected.Id).Sum(d => d.Amount):C}";
        ReconList.ItemsSource = _items.Where(p => p.Status == "Active").Select(p =>
        {
            var dep = _deposits.Where(d => d.PreneedId == p.Id).Sum(d => d.Amount);
            var rem = p.TotalAmount - dep;
            return $"{p.Number} ({p.Beneficiary}): {p.TotalAmount:C} contracted - {dep:C} deposited - {(rem <= 0 ? "RECONCILED" : $"{rem:C} UNDEPOSITED")}";
        }).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as PreneedContract;
        if (_selected is null) return;
        PurchaserBox.Text = _selected.Purchaser;
        BeneficiaryBox.Text = _selected.Beneficiary;
        DatePicker.Date = _selected.Date;
        TotalBox.Text = _selected.TotalAmount.ToString("0.##");
        FundingCombo.SelectedItem = _selected.FundingType;
        FundingRefBox.Text = _selected.FundingRef;
        StatusCombo.SelectedItem = _selected.Status;
        NotesBox.Text = _selected.Notes;
        RefreshAll();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new PreneedContract
        {
            Number = $"PN-{DateTime.Now.Year}-{_items.Count + 1:000}",
        };
        item.Purchaser = PurchaserBox.Text.Trim();
        item.Beneficiary = BeneficiaryBox.Text.Trim();
        item.Date = DatePicker.Date;
        item.TotalAmount = decimal.TryParse(TotalBox.Text.Trim().TrimStart('$'), out var v) ? v : 0m;
        item.FundingType = FundingCombo.SelectedItem as string ?? "Trust";
        item.FundingRef = FundingRefBox.Text.Trim();
        item.Status = StatusCombo.SelectedItem as string ?? "Active";
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshAll();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(p => p.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshAll();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
    }

    private void ClearForm()
    {
        PurchaserBox.Text = string.Empty;
        BeneficiaryBox.Text = string.Empty;
        TotalBox.Text = string.Empty;
        FundingRefBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        DepositSummaryText.Text = string.Empty;
    }

    private async void AddDepButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _deposits.Add(new DepositEntry
        {
            PreneedId = _selected.Id,
            Date = DateTimeOffset.Now,
            Amount = decimal.TryParse(DepAmountBox.Text.Trim().TrimStart('$'), out var v) ? v : 0m,
            Destination = DepDestBox.Text.Trim(),
        });
        await _depositStore.SaveAsync(_deposits);
        DepAmountBox.Text = string.Empty;
        DepDestBox.Text = string.Empty;
        RefreshAll();
    }

    private async void RemoveDepButton_Click(object sender, RoutedEventArgs e)
    {
        if (DepositsList.SelectedItem is not DepositEntry dep) return;
        _deposits.RemoveAll(d => d.Id == dep.Id);
        await _depositStore.SaveAsync(_deposits);
        RefreshAll();
    }
}
