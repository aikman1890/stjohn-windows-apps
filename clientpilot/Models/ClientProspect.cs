namespace ClientPilot.Models;

/// <summary>
/// One BCCS client prospect: who was pitched, when, by what route, and
/// where the reply stands, with a follow-up date and overdue flag.
/// </summary>
public sealed class ClientProspect
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Company { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public DateTimeOffset PitchDate { get; set; } = DateTimeOffset.Now;
    public string PitchType { get; set; } = "Email pitch";
    public string Status { get; set; } = "Pitched";
    public DateTimeOffset? FollowUpDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public bool IsActive => Status is not ("Not a fit" or "Closed");

    public string DisplayLine => Company;

    public string SubLine =>
        $"{Status} - pitched {PitchDate:MMM d, yyyy}" +
        (string.IsNullOrWhiteSpace(PitchType) ? "" : $" - {PitchType}") +
        (string.IsNullOrWhiteSpace(ContactEmail) ? "" : $" - {ContactEmail}");

    public string FollowUpText => FollowUpDate is { } f
        ? (f.Date >= DateTimeOffset.Now.Date
            ? $"Follow up by {f:MMM d, yyyy}"
            : $"Follow-up OVERDUE - was due {f:MMM d, yyyy}")
        : "No follow-up set";
}
