namespace BillPilot.Models;

/// <summary>
/// One household bill or utility account: amount due, due date, and any
/// disputed amount with its dispute status and case/reference number.
/// </summary>
public sealed class BillItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Provider { get; set; } = string.Empty;
    public string AccountLabel { get; set; } = string.Empty;
    public string AmountDue { get; set; } = string.Empty;
    public DateTimeOffset DueDate { get; set; } = DateTimeOffset.Now;
    public string AmountDisputed { get; set; } = string.Empty;
    public string Status { get; set; } = "Current";
    public string CaseRef { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public bool IsClosed => Status is "Paid" or "Closed";

    public int DaysRemaining => (DueDate.Date - DateTimeOffset.Now.Date).Days;

    public string DisplayLine => Provider;

    public string SubLine =>
        (string.IsNullOrWhiteSpace(AccountLabel) ? "" : $"{AccountLabel} - ") +
        (string.IsNullOrWhiteSpace(AmountDue) ? "" : $"Amount due {AmountDue}") +
        (string.IsNullOrWhiteSpace(AmountDisputed) ? "" : $" - disputed {AmountDisputed}") +
        (string.IsNullOrWhiteSpace(CaseRef) ? "" : $" - Ref {CaseRef}");

    public string DueText => IsClosed
        ? $"Was due {DueDate:MMM d, yyyy} - {Status}"
        : DaysRemaining >= 0
            ? $"Due {DueDate:MMM d, yyyy} - {DaysRemaining} day(s) left"
            : $"PAST DUE by {-DaysRemaining} day(s) - was due {DueDate:MMM d, yyyy}";
}
