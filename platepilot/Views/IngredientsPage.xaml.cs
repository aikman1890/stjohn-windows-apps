using PlatePilot.Models;
using PlatePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlatePilot.Views;

public sealed partial class IngredientsPage : Page
{
    private readonly JsonListStore<Ingredient> _store = new("ingredients.json");
    private List<Ingredient> _items = new();
    private Ingredient? _selected;

    public IngredientsPage()
    {
        this.InitializeComponent();
        BaseUnitCombo.ItemsSource = new[] { "oz", "lb", "each", "fl oz", "g", "kg" };
        BaseUnitCombo.SelectedIndex = 0;
        PriceBox.TextChanged += (_, _) => UpdatePreview();
        AmountBox.TextChanged += (_, _) => UpdatePreview();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(i => i.Name).ToList();
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private void UpdatePreview()
    {
        var price = ParseDec(PriceBox.Text);
        var amount = ParseDec(AmountBox.Text);
        CostPreviewText.Text = amount > 0
            ? $"Cost per base unit: {price / amount:C4}"
            : string.Empty;
    }

    private void RefreshHistory()
    {
        HistoryList.ItemsSource = _selected?.PriceHistory
            .OrderByDescending(p => p.Date)
            .Select(p => $"{p.Date:MMM d, yyyy} - {p.Price:C}{(string.IsNullOrWhiteSpace(p.Note) ? "" : $" - {p.Note}")}")
            .ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Ingredient;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        PurchaseUnitBox.Text = _selected.PurchaseUnit;
        PriceBox.Text = _selected.PurchasePrice.ToString("0.##");
        AmountBox.Text = _selected.PurchaseUnitAmount.ToString("0.###");
        BaseUnitCombo.SelectedItem = _selected.BaseUnit;
        SupplierBox.Text = _selected.Supplier;
        NotesBox.Text = _selected.Notes;
        RefreshHistory();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new Ingredient();
        var newPrice = ParseDec(PriceBox.Text);
        var priceChanged = _selected is not null && newPrice != item.PurchasePrice;
        item.Name = NameBox.Text.Trim();
        item.PurchaseUnit = PurchaseUnitBox.Text.Trim();
        item.PurchasePrice = newPrice;
        item.PurchaseUnitAmount = ParseDec(AmountBox.Text) is var a && a > 0 ? a : 1m;
        item.BaseUnit = BaseUnitCombo.SelectedItem as string ?? "oz";
        item.Supplier = SupplierBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null)
        {
            item.PriceHistory.Add(new PricePoint { Date = DateTimeOffset.Now, Price = newPrice, Note = "Initial price" });
            _items.Add(item);
        }
        else if (priceChanged)
        {
            item.PriceHistory.Add(new PricePoint { Date = DateTimeOffset.Now, Price = newPrice, Note = "Price updated" });
        }
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        RefreshHistory();
    }

    private async void RecordPriceButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        var price = ParseDec(NewPriceBox.Text);
        if (price <= 0) return;
        _selected.PurchasePrice = price;
        _selected.PriceHistory.Add(new PricePoint
        {
            Date = DateTimeOffset.Now,
            Price = price,
            Note = NewPriceNoteBox.Text.Trim(),
        });
        await _store.SaveAsync(_items);
        PriceBox.Text = price.ToString("0.##");
        NewPriceBox.Text = string.Empty;
        NewPriceNoteBox.Text = string.Empty;
        RefreshList();
        RefreshHistory();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(i => i.Id == _selected.Id);
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
        NameBox.Text = string.Empty;
        PurchaseUnitBox.Text = string.Empty;
        PriceBox.Text = string.Empty;
        AmountBox.Text = string.Empty;
        SupplierBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        HistoryList.ItemsSource = null;
    }
}
