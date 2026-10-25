namespace EntityPilot.Models;

public sealed class DocRef
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string EntityId { get; set; } = string.Empty;
    public string EntityLabel { get; set; } = string.Empty;
    public string Kind { get; set; } = "Formation"; // Formation / Operating agreement or bylaws / EIN letter / Annual filing / Other
    public string Title { get; set; } = string.Empty;
    /// <summary>Local path or reference only - EntityPilot indexes the owner's files; it fetches nothing.</summary>
    public string LocalPath { get; set; } = string.Empty;
    public DateTimeOffset? FiledDate { get; set; }

    public string DisplayLine =>
        $"{Kind} - {Title} - {EntityLabel}{(FiledDate is { } f ? $" - filed {f:MMM d, yyyy}" : string.Empty)} - {LocalPath}";
}
