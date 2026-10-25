namespace GuardPilot.Models;

public sealed class Shift
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PostId { get; set; } = string.Empty;
    public string PostLabel { get; set; } = string.Empty;
    public string SiteLabel { get; set; } = string.Empty;
    public decimal BillRatePerHour { get; set; }
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    /// <summary>Start/end as 24h decimals (18.5 = 18:30). End may exceed 24 for overnight shifts.</summary>
    public decimal StartHour { get; set; } = 18m;
    public decimal EndHour { get; set; } = 30m;
    public string GuardId { get; set; } = string.Empty;
    public string GuardLabel { get; set; } = string.Empty;
    public decimal? ActualHours { get; set; }
    public string Notes { get; set; } = string.Empty;

    public decimal ScheduledHours => EndHour > StartHour ? EndHour - StartHour : 0m;
    public bool Unfilled => string.IsNullOrWhiteSpace(GuardId);

    public string DisplayLine =>
        $"{Date:ddd, MMM d} {StartHour:0.0}-{EndHour:0.0}h - {PostLabel} - {(Unfilled ? "UNFILLED" : GuardLabel)} - sched {ScheduledHours:0.0}h" +
        (ActualHours is { } a ? $" - actual {a:0.0}h" : string.Empty);
}
