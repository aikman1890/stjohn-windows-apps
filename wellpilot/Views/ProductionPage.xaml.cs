using System.Globalization;
using WellPilot.Models;
using WellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WellPilot.Views;

public sealed partial class ProductionPage : Page
{
    private readonly JsonListStore<ProductionMonth> _store = new("production.json");
    private readonly JsonListStore<Well> _wellStore = new("wells.json");
    private List<ProductionMonth> _items = new();
    private List<Well> _wells = new();
    private ProductionMonth? _selected;

    public ProductionPage()
    {
        this.InitializeComponent();
        MonthBox.Text = DateTimeOffset.Now.AddMonths(-1).ToString("yyyy-MM");
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _wells = await _wellStore.LoadAsync();
        WellPicker.ItemsSource = _wells.Select(w => w.DisplayLine).ToList();
        if (_wells.Count > 0) WellPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(p => p.Month).ThenBy(p => p.WellLabel).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as ProductionMonth;
        if (_selected is null) return;
        var idx = _wells.FindIndex(w => w.Id == _selected.WellId);
        if (idx >= 0) WellPicker.SelectedIndex = idx;
        MonthBox.Text = _selected.Month;
        OilBox.Text = _selected.OilBbl.ToString(CultureInfo.InvariantCulture);
        GasBox.Text = _selected.GasMcf.ToString(CultureInfo.InvariantCulture);
        GrossBox.Text = _selected.GrossRevenue.ToString(CultureInfo.InvariantCulture);
        SevBox.Text = _selected.SeveranceTax.ToString(CultureInfo.InvariantCulture);
        DedBox.Text = _selected.Deductions.ToString(CultureInfo.InvariantCulture);
        PurchaserBox.Text = _selected.PurchaserRef;
        NotesBox.Text = _selected.Notes;
    }

    private decimal ParseNum(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (WellPicker.SelectedIndex < 0 || WellPicker.SelectedIndex >= _wells.Count) return;
        var well = _wells[WellPicker.SelectedIndex];
        var item = _selected ?? new ProductionMonth();
        item.WellId = well.Id;
        item.WellLabel = well.DisplayLine;
        item.Month = MonthBox.Text.Trim();
        item.OilBbl = ParseNum(OilBox.Text);
        item.GasMcf = ParseNum(GasBox.Text);
        item.GrossRevenue = ParseNum(GrossBox.Text);
        item.SeveranceTax = ParseNum(SevBox.Text);
        item.Deductions = ParseNum(DedBox.Text);
        item.PurchaserRef = PurchaserBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(p => p.Id == _selected.Id);
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
        OilBox.Text = string.Empty;
        GasBox.Text = string.Empty;
        GrossBox.Text = string.Empty;
        SevBox.Text = string.Empty;
        DedBox.Text = string.Empty;
        PurchaserBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }
}
