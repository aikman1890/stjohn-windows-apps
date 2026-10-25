namespace StablePilot.Models;

/// <summary>Training/session log - itemized training charges feed the monthly owner bill.</summary>
public sealed class SessionEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string HorseId { get; set; } = string.Empty;
    public string HorseLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Trainer { get; set; } = string.Empty;
    public string Kind { get; set; } = "Training session"; // Training session / Lesson / Exercise ride
    public decimal Fee { get; set; }
    public string Note { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {HorseLabel} - {Kind} with {Trainer} - {Fee:0.00} - {Note}";
}
