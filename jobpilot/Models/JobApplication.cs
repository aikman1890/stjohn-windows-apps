namespace JobPilot.Models;

/// <summary>
/// One job application or recruiter thread being tracked.
/// </summary>
public sealed class JobApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Rate { get; set; } = string.Empty;
    public string Status { get; set; } = "Applied";
    public DateTimeOffset DateApplied { get; set; } = DateTimeOffset.Now;
    public string Confirmation { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public DateTimeOffset? FollowUpDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public bool IsActive => Status is not ("Rejected" or "Withdrawn" or "Accepted");

    public string DisplayLine => $"{Company} - {Role}";

    public string SubLine =>
        $"{Status} - applied {DateApplied:MMM d, yyyy}" +
        (string.IsNullOrWhiteSpace(Rate) ? "" : $" - {Rate}") +
        (string.IsNullOrWhiteSpace(Contact) ? "" : $" - {Contact}");

    public string FollowUpText => FollowUpDate is { } f
        ? (f.Date >= DateTimeOffset.Now.Date
            ? $"Follow up by {f:MMM d, yyyy}"
            : $"Follow-up OVERDUE - was due {f:MMM d, yyyy}")
        : "No follow-up set";
}
