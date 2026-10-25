using LienPilot.Models;
using LienPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LienPilot.Views;

public sealed partial class SalesPage : Page
{
    private readonly JsonListStore<SaleRecord> _saleStore = new("sales.json");
    private readonly JsonListStore<StorageUnit> _unitStore = new("units.json");
    private readonly JsonListStore<StateProfile> _profileStore = new("profiles.json");
    private readonly JsonListStore<LienLedgerEntry> _ledgerStore = new("ledger.json");
    private readonly FacilitySettingsStore _settingsStore = new();
    private List<StorageUnit> _units = new();
    private List<SaleRecord> _sales = new();

    public SalesPage()
    {
        this.InitializeComponent();
        ExcessNoticePicker.Date = DateTimeOffset.Now;
        ExcessClaimedPicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _units = await _unitStore.LoadAsync();
        _sales = await _saleStore.LoadAsync();
        UnitPicker.ItemsSource = _units.Select(u => u.DisplayLine).ToList();
        if (_units.Count > 0) UnitPicker.SelectedIndex = 0;
        await BuildWorksheetAsync();
        RefreshSales();
    }

    private async Task BuildWorksheetAsync()
    {
        var settings = await _settingsStore.LoadAsync();
        var profiles = await _profileStore.LoadAsync();
        var profile = profiles.FirstOrDefault(p => p.Id == settings.ActiveProfileId) ?? profiles.FirstOrDefault();
        var ledger = await _ledgerStore.LoadAsync();
        var cleared = new List<string>();
        var held = new List<string>();
        if (profile is null)
        {
            ClearedText.Text = "No state profile - the worksheet cannot clear any unit. Create and confirm a profile in Settings.";
            ClearedList.ItemsSource = cleared;
            HeldList.ItemsSource = held;
            return;
        }
        foreach (var u in _units.Where(u => u.IsDelinquent && u.DefaultDate is not null).OrderBy(u => u.UnitNumber))
        {
            var tl = LienClock.Compute(u, profile, ledger);
            if (tl.SaleReady) cleared.Add($"Unit {u.UnitNumber} - {u.TenantName} - claim {u.Balance:C} - {tl.SaleBasis}");
            else held.Add($"Unit {u.UnitNumber} - {u.TenantName} - HELD: {string.Join(" ", tl.BlockReasons)}");
        }
        ClearedText.Text = $"{cleared.Count} unit(s) cleared, {held.Count} held.";
        ClearedList.ItemsSource = cleared;
        HeldList.ItemsSource = held;
    }

    private void RefreshSales()
    {
        SalesList.ItemsSource = null;
        SalesList.ItemsSource = _sales.OrderByDescending(s => s.Date).ToList();
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private async void RecordButton_Click(object sender, RoutedEventArgs e)
    {
        if (UnitPicker.SelectedIndex < 0 || UnitPicker.SelectedIndex >= _units.Count) return;
        var unit = _units[UnitPicker.SelectedIndex];
        var record = new SaleRecord
        {
            UnitId = unit.Id,
            UnitLabel = unit.DisplayLine,
            Date = DateTimeOffset.Now,
            SalePrice = ParseDec(PriceBox.Text),
            AppliedToClaim = ParseDec(AppliedBox.Text),
            ExcessNoticeDate = ExcessNoticeCheck.IsChecked == true ? ExcessNoticePicker.Date : null,
            ExcessClaimedDate = ExcessClaimedCheck.IsChecked == true ? ExcessClaimedPicker.Date : null,
            Notes = NotesBox.Text.Trim(),
        };
        _sales.Add(record);
        await _saleStore.SaveAsync(_sales);

        var units = await _unitStore.LoadAsync();
        var stored = units.FirstOrDefault(u => u.Id == unit.Id);
        if (stored is not null)
        {
            stored.Status = "Sold";
            stored.Balance = Math.Max(0m, stored.Balance - record.AppliedToClaim);
            await _unitStore.SaveAsync(units);
            _units = units;
        }
        StatusText.Text = $"Sale recorded for unit {unit.UnitNumber}: {record.SalePrice:C}, excess {record.Excess:C} tracked in the log.";
        RefreshSales();
        await BuildWorksheetAsync();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (SalesList.SelectedItem is not SaleRecord record) return;
        _sales.RemoveAll(s => s.Id == record.Id);
        await _saleStore.SaveAsync(_sales);
        RefreshSales();
    }
}
