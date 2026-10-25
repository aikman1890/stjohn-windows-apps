namespace PlatePilot.Models;

public sealed class PricePoint
{
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public decimal Price { get; set; }
    public string Note { get; set; } = string.Empty;
}

public sealed class Ingredient
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty; // e.g. "50 lb bag", "case of 12"
    public decimal PurchasePrice { get; set; }
    public decimal PurchaseUnitAmount { get; set; } = 1m; // how many base units the purchase unit contains
    public string BaseUnit { get; set; } = "oz"; // oz / lb / each / fl oz / g
    public string Supplier { get; set; } = string.Empty;
    public List<PricePoint> PriceHistory { get; set; } = new();
    public string Notes { get; set; } = string.Empty;

    /// <summary>Cost per base unit = purchase price / base units in the purchase unit. Decimal, full precision.</summary>
    public decimal CostPerBaseUnit => PurchaseUnitAmount > 0 ? PurchasePrice / PurchaseUnitAmount : 0m;

    public string DisplayLine =>
        $"{Name} - {PurchasePrice:C} per {PurchaseUnit} = {CostPerBaseUnit:C4}/{BaseUnit}" +
        (string.IsNullOrWhiteSpace(Supplier) ? string.Empty : $" - {Supplier}");

    /// <summary>Signed percent change from the previous recorded price to the current one.</summary>
    public decimal? LastChangePercent
    {
        get
        {
            if (PriceHistory.Count < 2) return null;
            var ordered = PriceHistory.OrderBy(p => p.Date).ToList();
            var prev = ordered[^2].Price;
            var cur = ordered[^1].Price;
            return prev > 0 ? Math.Round((cur - prev) / prev * 100m, 1) : null;
        }
    }
}
