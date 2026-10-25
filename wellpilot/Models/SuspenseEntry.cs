namespace WellPilot.Models;

public sealed class SuspenseEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public decimal Amount { get; set; }
    /// <summary>User-entered reason (unleased, minimum royalty, title, other).</summary>
    public string Reason { get; set; } = string.Empty;
    public bool Released { get; set; }
    public DateTimeOffset? ReleaseDate { get; set; }

    public string DisplayLine =>
        $"{OwnerLabel} - {Amount:C} - held {Date:MMM d, yyyy} - {Reason}" +
        (Released ? $" - RELEASED {ReleaseDate:MMM d, yyyy}" : " - IN SUSPENSE");
}
