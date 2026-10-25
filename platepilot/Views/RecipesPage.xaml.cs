using System.Net;
using System.Text;
using PlatePilot.Models;
using PlatePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PlatePilot.Views;

public sealed partial class RecipesPage : Page
{
    private readonly JsonListStore<Recipe> _store = new("recipes.json");
    private readonly JsonListStore<Ingredient> _ingredientStore = new("ingredients.json");
    private List<Recipe> _items = new();
    private List<Ingredient> _ingredients = new();
    private Recipe? _selected;
    private List<RecipeLine> _editLines = new();

    public RecipesPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _ingredients = await _ingredientStore.LoadAsync();
        IngredientPicker.ItemsSource = _ingredients.Select(i => i.Name).ToList();
        if (_ingredients.Count > 0) IngredientPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private Dictionary<string, decimal> CostMap() =>
        _ingredients.ToDictionary(i => i.Id, i => i.CostPerBaseUnit);

    private void RefreshList()
    {
        var map = CostMap();
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(r => r.Name).Select(r =>
        {
            var cost = r.CostWith(map);
            return $"{r.Name} - plate cost {cost:C} - food cost {r.FoodCostPercent(cost):0.#}% of {r.MenuPrice:C} (target {r.TargetFoodCostPercent:0.#}%)";
        }).ToList();
    }

    private void RefreshLines()
    {
        var map = CostMap();
        LinesList.ItemsSource = null;
        LinesList.ItemsSource = _editLines.Select(l =>
        {
            var cpu = map.TryGetValue(l.IngredientId, out var c) ? c : 0m;
            return $"{l.IngredientName} - {l.Quantity:0.###} AP @ yield {l.YieldPercent:0.#}% = {l.EffectiveQuantity:0.###} eff - {l.EffectiveQuantity * cpu:C}";
        }).ToList();
        UpdateCostText();
    }

    private void UpdateCostText()
    {
        var probe = new Recipe { Lines = _editLines, MenuPrice = ParseDec(MenuPriceBox.Text), TargetFoodCostPercent = ParseDec(TargetBox.Text) is var t && t > 0 ? t : 30m };
        var cost = probe.CostWith(CostMap());
        CostText.Text = _editLines.Count == 0
            ? string.Empty
            : $"Plate cost {cost:C} - food cost {probe.FoodCostPercent(cost):0.#}% at menu {probe.MenuPrice:C} (target {probe.TargetFoodCostPercent:0.#}%)";
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemsList.SelectedIndex < 0) return;
        var ordered = _items.OrderBy(r => r.Name).ToList();
        if (ItemsList.SelectedIndex >= ordered.Count) return;
        _selected = ordered[ItemsList.SelectedIndex];
        NameBox.Text = _selected.Name;
        CategoryBox.Text = _selected.Category;
        MenuPriceBox.Text = _selected.MenuPrice.ToString("0.##");
        TargetBox.Text = _selected.TargetFoodCostPercent.ToString("0.#");
        NotesBox.Text = _selected.Notes;
        _editLines = _selected.Lines.Select(l => new RecipeLine
        {
            Id = l.Id,
            IngredientId = l.IngredientId,
            IngredientName = l.IngredientName,
            Quantity = l.Quantity,
            YieldPercent = l.YieldPercent,
        }).ToList();
        RefreshLines();
    }

    private void AddLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (IngredientPicker.SelectedIndex < 0 || IngredientPicker.SelectedIndex >= _ingredients.Count) return;
        var ing = _ingredients[IngredientPicker.SelectedIndex];
        _editLines.Add(new RecipeLine
        {
            IngredientId = ing.Id,
            IngredientName = ing.Name,
            Quantity = ParseDec(QtyBox.Text),
            YieldPercent = ParseDec(YieldBox.Text) is var y && y > 0 ? y : 100m,
        });
        QtyBox.Text = "0";
        YieldBox.Text = "100";
        RefreshLines();
    }

    private void RemoveLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (LinesList.SelectedIndex >= 0 && LinesList.SelectedIndex < _editLines.Count)
        {
            _editLines.RemoveAt(LinesList.SelectedIndex);
            RefreshLines();
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new Recipe();
        item.Name = NameBox.Text.Trim();
        item.Category = CategoryBox.Text.Trim();
        item.MenuPrice = ParseDec(MenuPriceBox.Text);
        item.TargetFoodCostPercent = ParseDec(TargetBox.Text) is var t && t > 0 ? t : 30m;
        item.Notes = NotesBox.Text.Trim();
        item.Lines = _editLines;
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        UpdateCostText();
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
        NameBox.Text = string.Empty;
        CategoryBox.Text = string.Empty;
        MenuPriceBox.Text = string.Empty;
        TargetBox.Text = "30";
        NotesBox.Text = string.Empty;
        _editLines = new List<RecipeLine>();
        RefreshLines();
        CostText.Text = string.Empty;
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null || App.WindowInstance is null) return;
        var settings = await new PlateSettingsStore().LoadAsync();
        var map = CostMap();
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"cost-sheet-{_selected.Name.Replace(' ', '-').ToLowerInvariant()}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildCostSheetHtml(_selected, _ingredients, map, settings));
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string BuildCostSheetHtml(Recipe r, List<Ingredient> ingredients, Dictionary<string, decimal> map, PlateSettings settings)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Cost Sheet - {Enc(r.Name)}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}th{background:#f0f0f0}.r{text-align:right}.tot{font-weight:bold}</style></head><body>");
        if (!string.IsNullOrWhiteSpace(settings.RestaurantName))
            sb.Append($"<h1>{Enc(settings.RestaurantName)}</h1>");
        sb.Append($"<h2>Cost Sheet - {Enc(r.Name)}</h2>");
        sb.Append($"<p>Menu price {r.MenuPrice:C} - Target food cost {r.TargetFoodCostPercent:0.#}%</p>");
        sb.Append("<table><tr><th>Ingredient</th><th class=\"r\">Qty AP</th><th class=\"r\">Yield %</th><th class=\"r\">Qty eff</th><th class=\"r\">Cost/unit</th><th class=\"r\">Line cost</th></tr>");
        foreach (var l in r.Lines)
        {
            var cpu = map.TryGetValue(l.IngredientId, out var c) ? c : 0m;
            var unit = ingredients.FirstOrDefault(i => i.Id == l.IngredientId)?.BaseUnit ?? "";
            sb.Append($"<tr><td>{Enc(l.IngredientName)}</td><td class=\"r\">{l.Quantity:0.###} {Enc(unit)}</td><td class=\"r\">{l.YieldPercent:0.#}%</td>" +
                      $"<td class=\"r\">{l.EffectiveQuantity:0.###}</td><td class=\"r\">{cpu:C4}</td><td class=\"r\">{l.EffectiveQuantity * cpu:C}</td></tr>");
        }
        var cost = r.CostWith(map);
        sb.Append($"<tr class=\"tot\"><td colspan=\"5\" class=\"r\">Plate cost</td><td class=\"r\">{cost:C}</td></tr>");
        sb.Append($"<tr class=\"tot\"><td colspan=\"5\" class=\"r\">Food cost %</td><td class=\"r\">{r.FoodCostPercent(cost):0.#}%</td></tr>");
        sb.Append("</table>");
        sb.Append($"<p>Generated {DateTime.Now:MMMM d, yyyy} by PlatePilot from the kitchen's own price book.</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
