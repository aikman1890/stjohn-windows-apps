namespace WellPilot.Models;

/// <summary>
/// One line of a well's interest deck (division order). Entries are versioned:
/// editing never rewrites history - superseded lines stay in the file with their
/// effective dates, and every distribution run snapshots the decimals it used.
/// </summary>
public sealed class DeckEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string WellId { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerLabel { get; set; } = string.Empty;
    public string InterestType { get; set; } = "Working interest"; // Royalty / Overriding royalty / Working interest
    /// <summary>Decimal interest, up to 8 places. Decimal math only - never float.</summary>
    public decimal DecimalInterest { get; set; }
    public DateTimeOffset EffectiveDate { get; set; } = DateTimeOffset.Now;
    public int Version { get; set; } = 1;
    public bool Active { get; set; } = true;
    public string Note { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{OwnerLabel} - {InterestType} - {DecimalInterest:0.00000000} - v{Version} eff {EffectiveDate:MMM d, yyyy}" +
        (Active ? string.Empty : " - SUPERSEDED");
}
