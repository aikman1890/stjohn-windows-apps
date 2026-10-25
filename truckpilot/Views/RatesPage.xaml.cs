using TruckPilot.Models;
using TruckPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TruckPilot.Views;

public sealed partial class RatesPage : Page
{
    private readonly JsonListStore<RateTable> _store = new("rates.json");
    private List<RateTable> _items = new();
    private RateTable? _selected;

    public RatesPage()
    {
        this.InitializeComponent();
        var now = DateTimeOffset.Now;
        YearBox.Text = now.Year.ToString();
        QuarterCombo.ItemsSource = new[] { "Q1", "Q2", "Q3", "Q4" };
        QuarterCombo.SelectedIndex = (now.Month - 1) / 3;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Quarter).ThenBy(r => r.Jurisdiction)
            .ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as RateTable;
        if (_selected is null) return;
        YearBox.Text = _selected.Year.ToString();
        QuarterCombo.SelectedIndex = _selected.Quarter - 1;
        JurisdictionBox.Text = _selected.Jurisdiction;
        RateBox.Text = _selected.RatePerGallon.ToString("0.####");
        SourceBox.Text = _selected.Source;
        ConfirmCheck.IsChecked = _selected.Confirmed;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(JurisdictionBox.Text) ||
            !decimal.TryParse(RateBox.Text.Trim().TrimStart('$'), out var rate) || rate <= 0)
        {
            StatusText.Text = "Enter the jurisdiction and the exact published rate first.";
            return;
        }
        var item = _selected ?? new RateTable();
        item.Year = int.TryParse(YearBox.Text.Trim(), out var y) ? y : DateTimeOffset.Now.Year;
        item.Quarter = QuarterCombo.SelectedIndex + 1;
        item.Jurisdiction = JurisdictionBox.Text.Trim();
        item.RatePerGallon = rate;
        item.Source = SourceBox.Text.Trim();
        var nowConfirmed = ConfirmCheck.IsChecked == true;
        if (nowConfirmed && !item.Confirmed) item.ConfirmedDate = DateTimeOffset.Now;
        item.Confirmed = nowConfirmed;
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        StatusText.Text = item.Confirmed
            ? $"Rate saved and confirmed for {item.Jurisdiction} ({item.Year} Q{item.Quarter})."
            : "Rate saved but NOT confirmed - the worksheet will not use it until you confirm it.";
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(r => r.Id == _selected.Id);
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
        JurisdictionBox.Text = string.Empty;
        RateBox.Text = string.Empty;
        SourceBox.Text = string.Empty;
        ConfirmCheck.IsChecked = false;
        StatusText.Text = string.Empty;
    }
}
