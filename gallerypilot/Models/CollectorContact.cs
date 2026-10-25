namespace GalleryPilot.Models;

public sealed class CollectorContact
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Interests { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public sealed class OfferEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CollectorId { get; set; } = string.Empty;
    public string CollectorLabel { get; set; } = string.Empty;
    public string ArtworkLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    /// <summary>Bought / Offered / Viewing.</summary>
    public string Kind { get; set; } = "Viewing";
    public string Note { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Kind} - {ArtworkLabel} - {Note}";
}
