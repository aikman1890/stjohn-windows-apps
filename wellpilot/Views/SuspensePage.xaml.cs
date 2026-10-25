using System.Globalization;
using WellPilot.Models;
using WellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WellPilot.Views;

public sealed partial class SuspensePage : Page
{
    private readonly JsonListStore<SuspenseEntry> _store = new("suspense.json");
    private readonly JsonListStore<InterestOwner> _ownerStore = new("owners.json");
    private List<SuspenseEntry> _items = new();
    private List<InterestOwner> _owners = new();
    private SuspenseEntry? _selected;

    public SuspensePage()
    {
        this.InitializeComponent();
        DatePickerCtl.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _owners = await _ownerStore.LoadAsync();
        OwnerPicker.ItemsSource = _owners.Select(o => o.Name).ToList();
        if (_owners.Count > 0) OwnerPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(s => s.Released).ThenByDescending(s => s.Date).ToList();
        TotalsText.Text = string.Join(Environment.NewLine, _items.Where(s => !s.Released)
            .GroupBy(s => s.OwnerLabel).Select(g => $"{g.Key}: {g.Sum(s => s.Amount):C} held"));
        if (string.IsNullOrWhiteSpace(TotalsText.Text)) TotalsText.Text = "Nothing in suspense.";
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as SuspenseEntry;
        if (_selected is null) return;
        var idx = _owners.FindIndex(o => o.Id == _selected.OwnerId);
        if (idx >= 0) OwnerPicker.SelectedIndex = idx;
        DatePickerCtl.Date = _selected.Date;
        AmountBox.Text = _selected.Amount.ToString(CultureInfo.InvariantCulture);
        ReasonBox.Text = _selected.Reason;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (OwnerPicker.SelectedIndex < 0 || OwnerPicker.SelectedIndex >= _owners.Count) return;
        var owner = _owners[OwnerPicker.SelectedIndex];
        var item = _selected ?? new SuspenseEntry();
        item.OwnerId = owner.Id;
        item.OwnerLabel = owner.Name;
        item.Date = DatePickerCtl.Date ?? DateTimeOffset.Now;
        item.Amount = decimal.TryParse(AmountBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var a) ? a : 0m;
        item.Reason = ReasonBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void ReleaseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.Released = true;
        _selected.ReleaseDate = DateTimeOffset.Now;
        await _store.SaveAsync(_items);
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(s => s.Id == _selected.Id);
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
        AmountBox.Text = string.Empty;
        ReasonBox.Text = string.Empty;
    }
}
