using System.Globalization;
using MovePilot.Models;
using MovePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MovePilot.Views;

public sealed partial class StoragePage : Page
{
    private readonly JsonListStore<StorageLot> _store = new("lots.json");
    private List<StorageLot> _items = new();
    private StorageLot? _selected;

    public StoragePage()
    {
        this.InitializeComponent();
        InPicker.Date = DateTimeOffset.Now;
        OutPicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(l => l.OutDate is not null).ThenBy(l => l.InDate).ToList();
        TotalsText.Text = $"In storage: {_items.Count(l => l.OutDate is null)} lot(s) - balances outstanding {_items.Where(l => l.OutDate is null).Sum(l => l.Balance):C}" +
                          $"{Environment.NewLine}All lots accrued: {_items.Sum(l => l.Accrued):C}";
    }

    private void OutCheck_Changed(object sender, RoutedEventArgs e)
    {
        OutPicker.IsEnabled = OutCheck.IsChecked == true;
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as StorageLot;
        if (_selected is null) return;
        CustomerBox.Text = _selected.Customer;
        LotBox.Text = _selected.LotId;
        InPicker.Date = _selected.InDate;
        RateBox.Text = _selected.MonthlyRate.ToString(CultureInfo.InvariantCulture);
        PaidBox.Text = _selected.PaidToDate.ToString(CultureInfo.InvariantCulture);
        OutCheck.IsChecked = _selected.OutDate is not null;
        if (_selected.OutDate is { } o) OutPicker.Date = o;
        NotesBox.Text = _selected.Notes;
    }

    private decimal ParseMoney(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new StorageLot();
        item.Customer = CustomerBox.Text.Trim();
        item.LotId = LotBox.Text.Trim();
        item.InDate = InPicker.Date ?? DateTimeOffset.Now;
        item.MonthlyRate = ParseMoney(RateBox.Text);
        item.PaidToDate = ParseMoney(PaidBox.Text);
        item.OutDate = OutCheck.IsChecked == true ? OutPicker.Date : null;
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(l => l.Id == _selected.Id);
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
        CustomerBox.Text = string.Empty;
        LotBox.Text = string.Empty;
        RateBox.Text = string.Empty;
        PaidBox.Text = string.Empty;
        OutCheck.IsChecked = false;
        NotesBox.Text = string.Empty;
    }
}
