namespace PlotPilot.Models;

public sealed class WorkLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string SpaceLabel { get; set; } = string.Empty;
    public string Kind { get; set; } = "Maintenance"; // Opening / Closing / Marker setting / Maintenance / Other
    public string Note { get; set; } = string.Empty;
    public string ByWhom { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Kind} - {SpaceLabel} - {Note} ({ByWhom})";
}
