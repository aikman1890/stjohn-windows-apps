namespace DenialPilot.Models;

/// <summary>
/// One phone call with the insurer: when, which representative, the call
/// reference number, what was promised, and the next follow-up date.
/// </summary>
public sealed class CallLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CaseId { get; set; }
    public string CaseLabel { get; set; } = string.Empty;
    public DateTimeOffset CallDate { get; set; } = DateTimeOffset.Now;
    public string RepName { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Promised { get; set; } = string.Empty;
    public DateTimeOffset? FollowUpDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{CallDate:MMM d, yyyy h:mm tt} - {RepName}" +
        (string.IsNullOrWhiteSpace(ReferenceNumber) ? "" : $" - Ref {ReferenceNumber}");

    public string SubLine =>
        CaseLabel +
        (string.IsNullOrWhiteSpace(Promised) ? "" : $" - Promised: {Promised}");

    public string FollowUpText => FollowUpDate is { } f
        ? (f.Date >= DateTimeOffset.Now.Date
            ? $"Follow up by {f:MMM d, yyyy}"
            : $"Follow-up OVERDUE - was due {f:MMM d, yyyy}")
        : "No follow-up set";
}
