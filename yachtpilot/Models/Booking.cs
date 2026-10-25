namespace YachtPilot.Models;

public sealed class Booking
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string VesselId { get; set; } = string.Empty;
    public string VesselLabel { get; set; } = string.Empty;
    /// <summary>Charter / Owner use / Yard.</summary>
    public string Kind { get; set; } = "Charter";
    public string ChartererName { get; set; } = string.Empty;
    public string Broker { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.Now.AddDays(7);
    public decimal Rate { get; set; }
    public string Currency { get; set; } = "USD";
    /// <summary>APA-style advance allowance: received vs. spent, as a simple ledger pair.</summary>
    public decimal ApaAdvance { get; set; }
    public decimal ApaSpent { get; set; }
    public string Status { get; set; } = "Booked"; // Booked / Completed / Cancelled
    public string Notes { get; set; } = string.Empty;

    public decimal ApaBalance => ApaAdvance - ApaSpent;

    public string DisplayLine =>
        $"{StartDate:MMM d} - {EndDate:MMM d, yyyy} - {VesselLabel} - {Kind}" +
        (Kind == "Charter" ? $" - {ChartererName} - {Rate:C} {Currency} - APA bal {ApaBalance:C}" : string.Empty) +
        $" - {Status}";
}
