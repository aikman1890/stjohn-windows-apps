namespace StablePilot.Models;

public sealed class Horse
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerLabel { get; set; } = string.Empty;
    public string Stall { get; set; } = string.Empty;
    public DateTimeOffset? ArrivalDate { get; set; }
    public string BreedAgeNote { get; set; } = string.Empty;
    public string FeedAm { get; set; } = string.Empty;
    public string FeedPm { get; set; } = string.Empty;
    public string Supplements { get; set; } = string.Empty;
    public string TurnoutGroup { get; set; } = string.Empty;
    /// <summary>Blanket rules by temperature, as the barn sets them.</summary>
    public string BlanketRule { get; set; } = string.Empty;
    public string VetLabel { get; set; } = string.Empty;
    public string FarrierLabel { get; set; } = string.Empty;
    public decimal BoardRateMonthly { get; set; }
    public string Currency { get; set; } = "USD";
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Name} - stall {Stall} - owner {OwnerLabel} - board {BoardRateMonthly:0.00} {Currency}/mo";
}
