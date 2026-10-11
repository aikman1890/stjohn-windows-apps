namespace ClaimPilot.Models;

/// <summary>An item in the evidence vault; the file itself is copied under LocalFolder/Vault.</summary>
public sealed class EvidenceItem
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime ItemDate { get; set; } = DateTime.Today;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public string ItemDateDisplay => ItemDate.ToString("MMMM d, yyyy");
}
