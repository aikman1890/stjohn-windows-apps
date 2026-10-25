namespace WheelPilot.Models;

/// <summary>Dated balance correction - balances are never silently edited.</summary>
public sealed class Adjustment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StudentId { get; set; } = string.Empty;
    public string StudentLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public decimal HoursDelta { get; set; }
    public string Reason { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {HoursDelta:+0.0;-0.0}h - {Reason} - {StudentLabel}";
}
