namespace StablePilot.Models;

/// <summary>Append-only health & incident record - kept as it happens, the way an owner (or an insurer) will ask for it.</summary>
public sealed class HealthEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string HorseId { get; set; } = string.Empty;
    public string HorseLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    /// <summary>Vet visit / Injury / Treatment / Lameness / Note.</summary>
    public string Kind { get; set; } = "Note";
    public string Detail { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {HorseLabel} - {Kind} - {Detail} ({Provider})";
}
