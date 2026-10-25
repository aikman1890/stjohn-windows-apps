using PlatePilot.Models;
using PlatePilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlatePilot.Views;

public sealed partial class ImpactPage : Page
{
    private readonly JsonListStore<Recipe> _recipeStore = new("recipes.json");
    private readonly JsonListStore<Ingredient> _ingredientStore = new("ingredients.json");
    private List<Ingredient> _ingredients = new();
    private List<Recipe> _recipes = new();

    public ImpactPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _ingredients = await _ingredientStore.LoadAsync();
        _recipes = await _recipeStore.LoadAsync();
        IngredientPicker.ItemsSource = _ingredients.Select(i => i.Name).ToList();
        if (_ingredients.Count > 0) IngredientPicker.SelectedIndex = 0;
        Recompute();
    }

    private void IngredientPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => Recompute();

    private void NewPriceBox_TextChanged(object sender, TextChangedEventArgs e) => Recompute();

    private void Recompute()
    {
        if (IngredientPicker.SelectedIndex < 0 || IngredientPicker.SelectedIndex >= _ingredients.Count)
        {
            RowsList.ItemsSource = null;
            return;
        }
        var ing = _ingredients[IngredientPicker.SelectedIndex];
        CurrentText.Text = $"Current: {ing.PurchasePrice:C} per {ing.PurchaseUnit} = {ing.CostPerBaseUnit:C4}/{ing.BaseUnit}";
        var newPrice = decimal.TryParse(NewPriceBox.Text.Trim().TrimStart('$'), out var v) ? v : ing.PurchasePrice;
        var newCpu = ing.PurchaseUnitAmount > 0 ? newPrice / ing.PurchaseUnitAmount : 0m;

        var rows = new List<string>();
        foreach (var recipe in _recipes.Where(r => r.Lines.Any(l => l.IngredientId == ing.Id)).OrderBy(r => r.Name))
        {
            var currentMap = _ingredients.ToDictionary(i => i.Id, i => i.CostPerBaseUnit);
            var currentCost = recipe.CostWith(currentMap);
            var newMap = new Dictionary<string, decimal>(currentMap) { [ing.Id] = newCpu };
            var newCost = recipe.CostWith(newMap);
            var curPct = recipe.FoodCostPercent(currentCost);
            var newPct = recipe.FoodCostPercent(newCost);
            var flag = newPct > recipe.TargetFoodCostPercent ? " - OVER TARGET" : string.Empty;
            rows.Add($"{recipe.Name}: plate {currentCost:C} ({curPct:0.#}%) -> {newCost:C} ({newPct:0.#}%) at unchanged menu {recipe.MenuPrice:C}{flag}");
        }
        RowsList.ItemsSource = rows.Count > 0
            ? rows
            : new List<string> { "No dish uses this ingredient yet." };
    }
}
