namespace DisputePilot.Models;

/// <summary>
/// One debt dispute: who it is with, when the dispute was sent, and where it
/// stands. The 30-day response deadline is derived from the sent date.
/// </summary>
public sealed class DisputeItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Creditor { get; set; } = string.Empty;
    public string AccountRef { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public DateTimeOffset SentDate { get; set; } = DateTimeOffset.Now;
    public string Status { get; set; } = "Awaiting validation";
    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset Deadline => SentDate.AddDays(30);

    public int DaysRemaining => (int)Math.Ceiling((Deadline - DateTimeOffset.Now).TotalDays);

    public string DisplayLine => $"{Creditor} - {Status}";

    public string SubLine =>
        $"Sent {SentDate:MMM d, yyyy}" +
        (string.IsNullOrWhiteSpace(AccountRef) ? "" : $" - Acct {AccountRef}") +
        (string.IsNullOrWhiteSpace(Amount) ? "" : $" - {Amount}");

    public string DeadlineText => Status is "Deleted from reports" or "Settled" or "Closed"
        ? $"Deadline was {Deadline:MMM d, yyyy}"
        : DaysRemaining >= 0
            ? $"Response deadline {Deadline:MMM d, yyyy} - {DaysRemaining} day(s) left"
            : $"OVERDUE by {-DaysRemaining} day(s) - deadline was {Deadline:MMM d, yyyy}";
}
