namespace EstatePilot.Models;

public sealed class EstateDoc
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PropertyId { get; set; } = string.Empty;
    public string PropertyLabel { get; set; } = string.Empty;
    /// <summary>Manual / Warranty / Service contract / Floor plan / Other.</summary>
    public string Kind { get; set; } = "Manual";
    public string Title { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;

    public string DisplayLine => $"{PropertyLabel} - {Kind} - {Title} - {LocalPath}";
}
