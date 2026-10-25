using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PlatePilot.Models;

public sealed class RecipeLine
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string IngredientId { get; set; } = string.Empty;
    public string IngredientName { get; set; } = string.Empty;
    public decimal Quantity { get; set; } // in the ingredient's base unit, AS PURCHASED (AP)
    public decimal YieldPercent { get; set; } = 100m; // edible portion yield; 100 = no loss

    /// <summary>Effective quantity the plate must cover after yield loss: AP qty / yield.</summary>
    public decimal EffectiveQuantity => YieldPercent > 0 ? Quantity / (YieldPercent / 100m) : Quantity;
}

public sealed class Recipe
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal MenuPrice { get; set; }
    public decimal TargetFoodCostPercent { get; set; } = 30m;
    public List<RecipeLine> Lines { get; set; } = new();
    public string Notes { get; set; } = string.Empty;

    public decimal CostWith(Dictionary<string, decimal> costPerBaseByIngredientId)
    {
        decimal total = 0m;
        foreach (var line in Lines)
        {
            if (costPerBaseByIngredientId.TryGetValue(line.IngredientId, out var cpu))
                total += line.EffectiveQuantity * cpu;
        }
        return Math.Round(total, 2);
    }

    public decimal FoodCostPercent(decimal plateCost) =>
        MenuPrice > 0 ? Math.Round(plateCost / MenuPrice * 100m, 1) : 0m;

    public Brush CostBrush(decimal plateCost)
    {
        var pct = FoodCostPercent(plateCost);
        if (pct <= TargetFoodCostPercent) return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        if (pct <= TargetFoodCostPercent + 5m) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
        return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
    }
}
