using TruckPilot.Models;
using TruckPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TruckPilot.Views;

public sealed partial class FuelPage : Page
{
    private readonly JsonListStore<FuelPurchase> _store = new("fuel.json");
    private List<FuelPurchase> _items = new();
    private FuelPurchase? _selected;

    public FuelPage()
    {
        this.InitializeComponent();
        DatePicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        var quarters = _items
            .Select(t => WorksheetCalculator.QuarterOf(t.Date))
            .Distinct()
            .OrderByDescending(q => q.Year).ThenByDescending(q => q.Quarter)
            .Select(q => $"{q.Year} Q{q.Quarter}")
            .ToList();
        QuarterFilter.ItemsSource = new[] { "All quarters" }.Concat(quarters).ToList();
        QuarterFilter.SelectedIndex = 0;
        RefreshList();
    }

    private void QuarterFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

    private void RefreshList()
    {
        var shown = _items;
        if (QuarterFilter.SelectedItem is string s && s != "All quarters")
        {
            var parts = s.Split(' ');
            var year = int.Parse(parts[0]);
            var quarter = int.Parse(parts[1].TrimStart('Q'));
            shown = _items.Where(t => WorksheetCalculator.InQuarter(t.Date, year, quarter)).ToList();
        }
        shown = shown.OrderByDescending(t => t.Date).ToList();
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = shown;
        TotalText.Text = $"{shown.Count} purchase(s) - {shown.Sum(f => f.Gallons):N3} gallons - {shown.Sum(f => f.TotalCost):C}.";
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as FuelPurchase;
        if (_selected is null) return;
        DatePicker.Date = _selected.Date;
        JurisdictionBox.Text = _selected.Jurisdiction;
        GallonsBox.Text = _selected.Gallons.ToString("0.###");
        CostBox.Text = _selected.TotalCost.ToString("0.##");
        ReceiptBox.Text = _selected.ReceiptRef;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new FuelPurchase();
        item.Date = DatePicker.Date;
        item.Jurisdiction = JurisdictionBox.Text.Trim();
        item.Gallons = ParseDec(GallonsBox.Text);
        item.TotalCost = ParseDec(CostBox.Text);
        item.ReceiptRef = ReceiptBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(f => f.Id == _selected.Id);
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
        DatePicker.Date = DateTimeOffset.Now;
        JurisdictionBox.Text = string.Empty;
        GallonsBox.Text = string.Empty;
        CostBox.Text = string.Empty;
        ReceiptBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }
}
