namespace StablePilot.Models;

public sealed class ExtraCharge
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string HorseId { get; set; } = string.Empty;
    public string HorseLabel { get; set; } = string.Empty;
    public string OwnerLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    /// <summary>Holding for vet/farrier / Supplies / Show fees advanced / Other extra.</summary>
    public string Category { get; set; } = "Other extra";
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public string DisplayLine => $"{Date:MMM d, yyyy} - {HorseLabel} - {Category} - {Description} - {Amount:0.00}";
}
