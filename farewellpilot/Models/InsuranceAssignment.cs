namespace FarewellPilot.Models;

public sealed class InsuranceAssignment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; set; } = string.Empty;
    public string CaseLabel { get; set; } = string.Empty;
    public string Insurer { get; set; } = string.Empty;
    public string PolicyRef { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Not filed"; // Not filed / Filed / Pending / Paid / Denied
    public DateTimeOffset? FiledDate { get; set; }
    public DateTimeOffset? PaidDate { get; set; }

    public string DisplayLine =>
        $"{CaseLabel} - {Insurer} {PolicyRef} - {Amount:C} - {Status}" +
        (FiledDate is { } f ? $" - filed {f:MMM d}" : string.Empty);
}
