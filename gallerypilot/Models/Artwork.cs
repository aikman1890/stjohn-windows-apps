namespace GalleryPilot.Models;

public sealed class Artwork
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Artist { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Medium { get; set; } = string.Empty;
    public string Dimensions { get; set; } = string.Empty;
    public string Edition { get; set; } = string.Empty;
    /// <summary>Gallery / Storage / On loan / With client.</summary>
    public string Location { get; set; } = "Gallery";
    /// <summary>Available / Reserved / Sold / Returned.</summary>
    public string Status { get; set; } = "Available";
    public decimal AskingPrice { get; set; }
    public string Currency { get; set; } = "USD";
    /// <summary>Image reference only - a local file path, never an embedded store.</summary>
    public string ImagePath { get; set; } = string.Empty;
    public string ConsignmentId { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Artist} - \"{Title}\" ({Year}) - {Medium} - {Status} @ {Location} - {AskingPrice:0.00} {Currency}";
}
