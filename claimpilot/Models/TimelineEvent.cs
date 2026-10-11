namespace ClaimPilot.Models;

/// <summary>One dated event on the evidence timeline, sorted by date.</summary>
public sealed class TimelineEvent
{
    public long Id { get; set; }
    public DateTime EventDate { get; set; } = DateTime.Today;
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public string EventDateDisplay => EventDate.ToString("MMMM d, yyyy");
}
