namespace EstatePilot.Models;

public sealed class ManualSection
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PropertyId { get; set; } = string.Empty;
    public string PropertyLabel { get; set; } = string.Empty;
    /// <summary>Room notes / Standing instruction / Principal preference / Seasonal procedure.</summary>
    public string Section { get; set; } = "Room notes";
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
