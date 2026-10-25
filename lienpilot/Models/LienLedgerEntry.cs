namespace LienPilot.Models;

public sealed class LienLedgerEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UnitId { get; set; } = string.Empty;
    public string UnitLabel { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Method { get; set; } = string.Empty; // Verified mail / Certified mail / Email / Hand delivery / Publication
    public string Tracking { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {StepType} - {Method}" +
        (string.IsNullOrWhiteSpace(Tracking) ? string.Empty : $" - {Tracking}") +
        (string.IsNullOrWhiteSpace(Note) ? string.Empty : $" - {Note}");
}
