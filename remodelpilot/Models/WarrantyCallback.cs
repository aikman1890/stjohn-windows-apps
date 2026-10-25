namespace RemodelPilot.Models;

public sealed class WarrantyCallback
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string JobId { get; set; } = string.Empty;
    public string JobLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Issue { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public decimal Cost { get; set; }

    public string DisplayLine => $"{Date:MMM d, yyyy} - {JobLabel} - {Issue}" +
        (string.IsNullOrWhiteSpace(Resolution) ? " - OPEN" : $" - {Resolution}") +
        (Cost > 0 ? $" - cost {Cost:C}" : string.Empty);
}
