namespace TaxPilot.Models;

public sealed class EfileLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string EngagementId { get; set; } = string.Empty;
    public string ClientLabel { get; set; } = string.Empty;
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.Now;
    public string Method { get; set; } = string.Empty;
    public string Result { get; set; } = "Submitted"; // Submitted / Accepted / Rejected
    public string RejectCode { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{ClientLabel} - {Result} - submitted {SubmittedAt:MMM d, yyyy h:mm tt} via {Method}" +
        (Result == "Rejected" ? $" - {RejectCode}: {Note}" : string.Empty);
}
