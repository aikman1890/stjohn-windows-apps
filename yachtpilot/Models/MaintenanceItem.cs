namespace YachtPilot.Models;

public sealed class MaintenanceItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string VesselId { get; set; } = string.Empty;
    public string VesselLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    /// <summary>Date / Engine hours.</summary>
    public string Basis { get; set; } = "Date";
    public DateTimeOffset? LastDoneDate { get; set; }
    public DateTimeOffset? NextDueDate { get; set; }
    public decimal? LastDoneHours { get; set; }
    public decimal? NextDueHours { get; set; }
    public string Note { get; set; } = string.Empty;

    public bool IsOverdue(decimal currentHours)
    {
        if (Basis == "Engine hours")
            return NextDueHours is { } h && currentHours >= h;
        return NextDueDate is { } d && d.Date < DateTimeOffset.Now.Date;
    }

    public string DisplayLine =>
        Basis == "Engine hours"
            ? $"{VesselLabel} - {Title} - due at {NextDueHours:0}h (last done {LastDoneHours:0}h)"
            : $"{VesselLabel} - {Title} - due {NextDueDate:MMM d, yyyy} (last done {LastDoneDate:MMM d, yyyy})";
}
