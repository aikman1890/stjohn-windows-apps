using PlatePilot.Models;
using Windows.Storage;

namespace PlatePilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("plate-seeded.flag") is not null) return;

        var ingredientStore = new JsonListStore<Ingredient>("ingredients.json");
        var recipeStore = new JsonListStore<Recipe>("recipes.json");
        if ((await ingredientStore.LoadAsync()).Count == 0)
        {
            var flour = new Ingredient
            {
                Name = "Sample Flour",
                PurchaseUnit = "50 lb bag",
                PurchasePrice = 22.50m,
                PurchaseUnitAmount = 800m,
                BaseUnit = "oz",
                Supplier = "Sample Supplier",
                PriceHistory = new List<PricePoint>
                {
                    new() { Date = DateTimeOffset.Now.AddMonths(-2), Price = 21.00m, Note = "Sample history point" },
                    new() { Date = DateTimeOffset.Now, Price = 22.50m, Note = "Sample current price" },
                },
            };
            var chicken = new Ingredient
            {
                Name = "Sample Chicken Breast",
                PurchaseUnit = "40 lb case",
                PurchasePrice = 68.00m,
                PurchaseUnitAmount = 640m,
                BaseUnit = "oz",
                Supplier = "Sample Supplier",
                PriceHistory = new List<PricePoint>
                {
                    new() { Date = DateTimeOffset.Now, Price = 68.00m, Note = "Sample current price" },
                },
            };
            await ingredientStore.SaveAsync(new List<Ingredient> { flour, chicken });
            await recipeStore.SaveAsync(new List<Recipe>
            {
                new()
                {
                    Name = "Sample Plate - Chicken and Bread Service",
                    Category = "Sample",
                    MenuPrice = 18.00m,
                    TargetFoodCostPercent = 30m,
                    Lines = new List<RecipeLine>
                    {
                        new() { IngredientId = chicken.Id, IngredientName = chicken.Name, Quantity = 8m, YieldPercent = 90m },
                        new() { IngredientId = flour.Id, IngredientName = flour.Name, Quantity = 4m, YieldPercent = 100m },
                    },
                    Notes = "Sample recipe - delete from the Recipes page.",
                },
            });
        }
        await folder.CreateFileAsync("plate-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
