namespace TaxPilot.Models;

public sealed class DocItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string EngagementId { get; set; } = string.Empty;
    public string ClientLabel { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Received { get; set; }
    public DateTimeOffset? ReceivedDate { get; set; }
    /// <summary>Where the office files it - path/reference note only; documents are not stored in TaxPilot.</summary>
    public string FiledRef { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{(Received ? "[in] " : "[MISSING] ")}{ClientLabel} - {Label}" +
        (Received ? $" - {ReceivedDate:MMM d}" : string.Empty);
}
