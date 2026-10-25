namespace MovePilot.Models;

public sealed class MoveEstimate
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string MoveId { get; set; } = string.Empty;
    public string CustomerLabel { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public string Type { get; set; } = "Hourly"; // Hourly / Flat / Not-to-exceed
    public int CrewSize { get; set; } = 3;
    public int Trucks { get; set; } = 1;
    public decimal HourlyRate { get; set; }
    public decimal EstHours { get; set; }
    public decimal FlatAmount { get; set; }
    public decimal TravelFee { get; set; }
    public decimal Materials { get; set; }
    public decimal OtherFees { get; set; }
    public string Terms { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Draft / Sent / Approved / Declined
    public DateTimeOffset? DecisionDate { get; set; }
    public string DecisionMethod { get; set; } = string.Empty;
    public string DecidedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;

    public decimal Amount => Type == "Hourly"
        ? HourlyRate * EstHours + TravelFee + Materials + OtherFees
        : FlatAmount + TravelFee + Materials + OtherFees;

    public string DisplayLine =>
        $"v{Version} {Type} - {Amount:C} - {Status}" +
        (Status is "Approved" or "Declined" && DecisionDate is { } d
            ? $" on {d:MMM d, yyyy} by {DecidedBy} ({DecisionMethod})"
            : $" - created {CreatedDate:MMM d, yyyy}") +
        $" - {CustomerLabel}";
}
