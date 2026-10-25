namespace GalleryPilot.Models;

/// <summary>Append-only in spirit: provenance, exhibition history, and condition entries are dated and never silently rewritten.</summary>
public sealed class RecordEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ArtworkId { get; set; } = string.Empty;
    public string ArtworkLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    /// <summary>Provenance / Exhibition / Publication / Condition.</summary>
    public string Kind { get; set; } = "Provenance";
    /// <summary>For condition entries: Intake / Before loan / At return / General.</summary>
    public string Moment { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {Kind}{(string.IsNullOrWhiteSpace(Moment) ? string.Empty : $" ({Moment})")} - {Detail}";
}
