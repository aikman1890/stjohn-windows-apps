namespace RemodelPilot.Models;

public sealed class ChangeOrder
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string JobId { get; set; } = string.Empty;
    public string JobLabel { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = "Draft"; // Draft / Sent / Approved / Declined
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? SentDate { get; set; }
    public DateTimeOffset? DecisionDate { get; set; }
    public string Method { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Number} - {JobLabel} - {Price:C} - {Status}" +
        (DecisionDate is { } d ? $" - decided {d:MMM d, yyyy}" : string.Empty);
    public string SubLine => Description;
}
