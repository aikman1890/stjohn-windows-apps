namespace RemodelPilot.Models;

public sealed class ActualCost
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string JobId { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Category} - {Description} - {Amount:C}" +
        (string.IsNullOrWhiteSpace(Vendor) ? string.Empty : $" - {Vendor}");
}
