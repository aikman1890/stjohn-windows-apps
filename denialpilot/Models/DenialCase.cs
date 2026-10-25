namespace DenialPilot.Models;

/// <summary>
/// One denied health-insurance claim being fought: the case file, the
/// appeal deadline engine (a filing clock computed from the denial date
/// with its basis shown), and the insurer's response clock once an appeal
/// is sent. DenialPilot organizes and documents - it is not medical or
/// legal advice, and it never promises an outcome.
/// </summary>
public sealed class DenialCase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PatientName { get; set; } = string.Empty;
    public string Insurer { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public DateTimeOffset ServiceDate { get; set; } = DateTimeOffset.Now;
    public string Provider { get; set; } = string.Empty;
    public string AmountDenied { get; set; } = string.Empty;
    public DateTimeOffset DenialDate { get; set; } = DateTimeOffset.Now;
    public string ReasonCategory { get; set; } = "Administrative";
    public string Status { get; set; } = "Denied";
    public string ClockPreset { get; set; } = "180-day appeal window";
    public int CustomDays { get; set; } = 180;
    public DateTimeOffset? AppealSentDate { get; set; }
    public bool UrgentClock { get; set; }
    public string Documents { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public bool IsResolved => Status is "Resolved - won" or "Resolved - lost" or "Resolved - paid";

    public DateTimeOffset AppealDeadline => ClockPreset switch
    {
        "4-month external review window" => DenialDate.AddMonths(4),
        "Custom days" => DenialDate.AddDays(CustomDays),
        _ => DenialDate.AddDays(180),
    };

    public string DeadlineBasis => ClockPreset switch
    {
        "4-month external review window" => "4 months from denial date, per your denial letter",
        "Custom days" => $"{CustomDays} days from denial date, per your denial letter",
        _ => "180 days from denial date, per your denial letter",
    };

    public int AppealDaysRemaining => (AppealDeadline.Date - DateTimeOffset.Now.Date).Days;

    public DateTimeOffset? ResponseDeadline => AppealSentDate is { } s
        ? (UrgentClock ? s.AddHours(72) : s.AddDays(30))
        : null;

    public int? ResponseDaysRemaining => ResponseDeadline is { } d
        ? (d.Date - DateTimeOffset.Now.Date).Days
        : null;

    public string DisplayLine => $"{Insurer} - Claim {ClaimNumber}";

    public string SubLine =>
        (string.IsNullOrWhiteSpace(PatientName) ? "" : $"{PatientName} - ") +
        (string.IsNullOrWhiteSpace(Provider) ? "" : $"{Provider} - ") +
        $"{ReasonCategory} - {Status}" +
        (string.IsNullOrWhiteSpace(AmountDenied) ? "" : $" - {AmountDenied} denied");

    public string AppealText => IsResolved
        ? $"Appeal deadline was {AppealDeadline:MMM d, yyyy}"
        : AppealDaysRemaining >= 0
            ? $"Appeal deadline {AppealDeadline:MMM d, yyyy} - {AppealDaysRemaining} day(s) left ({DeadlineBasis})"
            : $"Appeal deadline OVERDUE by {-AppealDaysRemaining} day(s) - was {AppealDeadline:MMM d, yyyy}";

    public string ResponseText => AppealSentDate is null
        ? "Appeal not sent yet - no response clock running"
        : IsResolved
            ? $"Insurer response was due {ResponseDeadline:MMM d, yyyy}"
            : ResponseDaysRemaining is { } r && r >= 0
                ? $"Insurer response due {ResponseDeadline:MMM d, yyyy} - {r} day(s) left ({(UrgentClock ? "72-hour urgent" : "30-day standard")} clock)"
                : $"Insurer response OVERDUE by {-ResponseDaysRemaining} day(s) - was due {ResponseDeadline:MMM d, yyyy}";
}
