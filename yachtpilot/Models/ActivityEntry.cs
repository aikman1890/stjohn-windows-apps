namespace YachtPilot.Models;

/// <summary>The proof ledger: what was actually done for this vessel, when, and how it was sent/done.</summary>
public sealed class ActivityEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string VesselId { get; set; } = string.Empty;
    public string VesselLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    /// <summary>Certificate renewed / Service completed / Charter run / Statement sent / Other.</summary>
    public string Kind { get; set; } = "Other";
    public string Detail { get; set; } = string.Empty;
    public string MethodNote { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {VesselLabel} - {Kind} - {Detail} ({MethodNote})";
}
