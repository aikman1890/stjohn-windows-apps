namespace YachtPilot.Models;

public sealed class Expense
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string VesselId { get; set; } = string.Empty;
    public string VesselLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Category { get; set; } = "Maintenance"; // Berth / Crew / Maintenance / Provisions / Fuel / Management fee / Insurance / Other
    /// <summary>Decimals with an explicit currency code - never floats, never assumed currency.</summary>
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaidBy { get; set; } = "Manager"; // Manager / Owner direct
    public string Note { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {VesselLabel} - {Category} - {Amount:0.00} {Currency} - {Note}";
}
