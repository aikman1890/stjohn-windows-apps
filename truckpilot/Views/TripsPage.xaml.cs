using TruckPilot.Models;
using TruckPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TruckPilot.Views;

public sealed partial class TripsPage : Page
{
    private readonly JsonListStore<Trip> _store = new("trips.json");
    private List<Trip> _items = new();
    private List<Trip> _shown = new();
    private Trip? _selected;

    public TripsPage()
    {
        this.InitializeComponent();
        DatePicker.Date = DateTimeOffset.Now;
        OdoStartBox.TextChanged += (_, _) => UpdatePreview();
        OdoEndBox.TextChanged += (_, _) => UpdatePreview();
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
        _shown = _items;
        if (QuarterFilter.SelectedItem is string s && s != "All quarters")
        {
            var parts = s.Split(' ');
            var year = int.Parse(parts[0]);
            var quarter = int.Parse(parts[1].TrimStart('Q'));
            _shown = _items.Where(t => WorksheetCalculator.InQuarter(t.Date, year, quarter)).ToList();
        }
        _shown = _shown.OrderByDescending(t => t.Date).ToList();
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _shown;
        TotalText.Text = $"{_shown.Count} trip(s) - {_shown.Sum(t => t.Miles):N1} miles total.";
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().Replace(",", ""), out var v) ? v : 0m;

    private void UpdatePreview()
    {
        var start = ParseDec(OdoStartBox.Text);
        var end = ParseDec(OdoEndBox.Text);
        MilesPreviewText.Text = end > start ? $"Segment miles: {end - start:N1}" : string.Empty;
        WarningText.Text = string.Empty;
        if (_items.Count > 0 && end > 0)
        {
            var maxEnd = _items.Where(t => _selected is null || t.Id != _selected.Id).Select(t => t.OdometerEnd).DefaultIfEmpty(0m).Max();
            if (start < maxEnd)
                WarningText.Text = $"Heads up: this start ({start:N0}) is below your highest recorded odometer end ({maxEnd:N0}) - possible overlap.";
            else if (start > maxEnd)
                WarningText.Text = $"Heads up: gap of {start - maxEnd:N0} miles after your last recorded end ({maxEnd:N0}) - make sure those miles get a record too.";
        }
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Trip;
        if (_selected is null) return;
        DatePicker.Date = _selected.Date;
        JurisdictionBox.Text = _selected.Jurisdiction;
        OdoStartBox.Text = _selected.OdometerStart.ToString("0.##");
        OdoEndBox.Text = _selected.OdometerEnd.ToString("0.##");
        RouteBox.Text = _selected.RouteNotes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new Trip();
        item.Date = DatePicker.Date;
        item.Jurisdiction = JurisdictionBox.Text.Trim();
        item.OdometerStart = ParseDec(OdoStartBox.Text);
        item.OdometerEnd = ParseDec(OdoEndBox.Text);
        item.RouteNotes = RouteBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(t => t.Id == _selected.Id);
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
        OdoStartBox.Text = string.Empty;
        OdoEndBox.Text = string.Empty;
        RouteBox.Text = string.Empty;
        MilesPreviewText.Text = string.Empty;
        WarningText.Text = string.Empty;
    }
}
