using System.Globalization;
using WellPilot.Models;
using WellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WellPilot.Views;

public sealed partial class ExpensesPage : Page
{
    private readonly JsonListStore<WellExpense> _store = new("expenses.json");
    private readonly JsonListStore<Well> _wellStore = new("wells.json");
    private readonly WellSettingsStore _settingsStore = new();
    private List<WellExpense> _items = new();
    private List<Well> _wells = new();
    private WellExpense? _selected;

    public ExpensesPage()
    {
        this.InitializeComponent();
        MonthBox.Text = DateTimeOffset.Now.AddMonths(-1).ToString("yyyy-MM");
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        CategoryCombo.ItemsSource = settings.ExpenseCategories;
        if (settings.ExpenseCategories.Count > 0) CategoryCombo.SelectedIndex = 0;
        _wells = await _wellStore.LoadAsync();
        WellPicker.ItemsSource = _wells.Select(w => w.DisplayLine).ToList();
        if (_wells.Count > 0) WellPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(x => x.Month).ThenBy(x => x.WellLabel).ToList();
        TotalsText.Text = string.Join(Environment.NewLine, _items
            .GroupBy(x => x.Month).OrderByDescending(g => g.Key)
            .Take(4).Select(g => $"{g.Key}: {g.Sum(x => x.Amount):C} across {g.Count()} expense line(s)"));
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as WellExpense;
        if (_selected is null) return;
        var idx = _wells.FindIndex(w => w.Id == _selected.WellId);
        if (idx >= 0) WellPicker.SelectedIndex = idx;
        MonthBox.Text = _selected.Month;
        CategoryCombo.Text = _selected.Category;
        AmountBox.Text = _selected.Amount.ToString(CultureInfo.InvariantCulture);
        VendorBox.Text = _selected.Vendor;
        NoteBox.Text = _selected.Note;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (WellPicker.SelectedIndex < 0 || WellPicker.SelectedIndex >= _wells.Count) return;
        var well = _wells[WellPicker.SelectedIndex];
        var item = _selected ?? new WellExpense();
        item.WellId = well.Id;
        item.WellLabel = well.DisplayLine;
        item.Month = MonthBox.Text.Trim();
        item.Category = CategoryCombo.Text.Trim();
        item.Amount = decimal.TryParse(AmountBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var a) ? a : 0m;
        item.Vendor = VendorBox.Text.Trim();
        item.Note = NoteBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(x => x.Id == _selected.Id);
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
        VendorBox.Text = string.Empty;
        NoteBox.Text = string.Empty;
    }
}
