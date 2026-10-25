using PlatePilot.Models;
using PlatePilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlatePilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<Recipe> _recipeStore = new("recipes.json");
    private readonly JsonListStore<Ingredient> _ingredientStore = new("ingredients.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var recipes = await _recipeStore.LoadAsync();
        var ingredients = await _ingredientStore.LoadAsync();
        var map = ingredients.ToDictionary(i => i.Id, i => i.CostPerBaseUnit);
        SummaryText.Text = $"{recipes.Count} dish(es) costed from {ingredients.Count} ingredient(s) in the price book.";

        var off = recipes
            .Select(r => (Recipe: r, Cost: r.CostWith(map)))
            .Select(x => (x.Recipe, x.Cost, Pct: x.Recipe.FoodCostPercent(x.Cost)))
            .Where(x => x.Pct > x.Recipe.TargetFoodCostPercent)
            .OrderByDescending(x => x.Pct - x.Recipe.TargetFoodCostPercent)
            .Select(x => $"{x.Recipe.Name} - food cost {x.Pct:0.#}% vs target {x.Recipe.TargetFoodCostPercent:0.#}% - plate {x.Cost:C} at menu {x.Recipe.MenuPrice:C}")
            .ToList();
        OffTargetList.ItemsSource = off.Count > 0 ? off : new List<string> { "Every dish is at or under its target. " };

        var movers = ingredients
            .Where(i => i.LastChangePercent is not null)
            .OrderByDescending(i => Math.Abs(i.LastChangePercent!.Value))
            .Select(i => $"{i.Name} - {(i.LastChangePercent!.Value >= 0 ? "+" : "")}{i.LastChangePercent:0.#}% - now {i.PurchasePrice:C} per {i.PurchaseUnit}")
            .ToList();
        MoversList.ItemsSource = movers.Count > 0 ? movers : new List<string> { "No price changes recorded yet - edit a price or use Record price on the Ingredients page." };
    }
}
