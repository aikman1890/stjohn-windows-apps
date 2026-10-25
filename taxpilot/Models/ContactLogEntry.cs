namespace TaxPilot.Models;

public sealed class ContactLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ClientId { get; set; } = string.Empty;
    public string ClientLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Who { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Promised { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {ClientLabel} - {Who}: {Summary}" +
        (string.IsNullOrWhiteSpace(Promised) ? string.Empty : $" [promised: {Promised}]");
}
