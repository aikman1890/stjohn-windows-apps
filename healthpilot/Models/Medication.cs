namespace HealthPilot.Models;

/// <summary>One medication on the current list.</summary>
public sealed class Medication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => string.IsNullOrWhiteSpace(Dosage) ? Name : $"{Name} - {Dosage}";
    public string SubLine =>
        (string.IsNullOrWhiteSpace(Schedule) ? "" : Schedule) +
        (string.IsNullOrWhiteSpace(Notes) ? "" : $" - {Notes}");
}
