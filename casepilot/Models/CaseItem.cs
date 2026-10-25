namespace CasePilot.Models;

/// <summary>
/// One court case: caption, court, judge, the next key deadline (with a
/// color-coded countdown), and the next hearing or conference.
/// </summary>
public sealed class CaseItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CaseName { get; set; } = string.Empty;
    public string CaseNumber { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string Judge { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTimeOffset NextDeadline { get; set; } = DateTimeOffset.Now.AddDays(30);
    public string DeadlineLabel { get; set; } = "Next deadline";
    public DateTimeOffset? HearingDate { get; set; }
    public string HearingType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public bool IsClosed => Status is "Closed" or "Dismissed" or "Settled" or "Judgment entered";

    public int DaysRemaining => (NextDeadline.Date - DateTimeOffset.Now.Date).Days;

    private string Label => string.IsNullOrWhiteSpace(DeadlineLabel) ? "Next deadline" : DeadlineLabel;

    public string DisplayLine => CaseName;

    public string SubLine =>
        (string.IsNullOrWhiteSpace(CaseNumber) ? "" : $"{CaseNumber} - ") +
        Court +
        (string.IsNullOrWhiteSpace(Judge) ? "" : $" - Judge {Judge}") +
        $" - {Status}";

    public string DeadlineText => IsClosed
        ? $"Deadline was {NextDeadline:MMM d, yyyy}"
        : DaysRemaining >= 0
            ? $"{Label} {NextDeadline:MMM d, yyyy} - {DaysRemaining} day(s) left"
            : $"OVERDUE by {-DaysRemaining} day(s) - {Label} was {NextDeadline:MMM d, yyyy}";

    public string HearingText => HearingDate is { } h
        ? $"{(string.IsNullOrWhiteSpace(HearingType) ? "Hearing" : HearingType)}: {h:MMM d, yyyy}"
        : "No hearing scheduled";
}
