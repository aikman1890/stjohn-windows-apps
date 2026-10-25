namespace GalleryPilot.Models;

public sealed class GallerySettings
{
    public string GalleryName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "USD";
    /// <summary>Standard artist-payment terms after a sale, in days.</summary>
    public int PaymentTermsDays { get; set; } = 30;
}
