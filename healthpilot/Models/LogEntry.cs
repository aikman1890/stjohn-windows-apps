namespace HealthPilot.Models;

/// <summary>
/// One health-log entry: a CPAP night, a symptom note, or a pain reading.
/// </summary>
public sealed class LogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Category { get; set; } = "CPAP";
    public int PainLevel { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Category}";
    public string SubLine =>
        (Category == "Pain" || PainLevel > 0 ? $"Pain {PainLevel}/10" : "CPAP / symptom note") +
        (string.IsNullOrWhiteSpace(Notes) ? "" : $" - {Notes}");
}
