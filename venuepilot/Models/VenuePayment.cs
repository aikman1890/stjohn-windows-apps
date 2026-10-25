namespace VenuePilot.Models;

public sealed class VenuePayment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string EventId { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public decimal Amount { get; set; }
    public string Kind { get; set; } = "Deposit"; // Deposit / Milestone / Final / Refund
    public string Note { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Kind} - {Amount:C} - {Note}";
}
