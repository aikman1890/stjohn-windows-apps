namespace FarewellPilot.Models;

public sealed class Payment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Amount:C} - {Method}" +
        (string.IsNullOrWhiteSpace(Note) ? string.Empty : $" - {Note}");
}
