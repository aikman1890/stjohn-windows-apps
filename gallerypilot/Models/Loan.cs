namespace GalleryPilot.Models;

public sealed class Loan
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ArtworkId { get; set; } = string.Empty;
    public string ArtworkLabel { get; set; } = string.Empty;
    public string Borrower { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset ReturnDue { get; set; } = DateTimeOffset.Now.AddMonths(2);
    public DateTimeOffset? ReturnedDate { get; set; }
    /// <summary>The condition pair: a loan cannot be closed without both ends documented.</summary>
    public bool ConditionOutLogged { get; set; }
    public bool ConditionInLogged { get; set; }
    public string Notes { get; set; } = string.Empty;

    public bool IsClosed => ReturnedDate is not null;
    public int DaysUntilReturn => (ReturnDue.Date - DateTimeOffset.Now.Date).Days;

    public string DisplayLine =>
        $"{ArtworkLabel} -> {Borrower} ({Venue}) - out {StartDate:MMM d, yyyy} - due back {ReturnDue:MMM d, yyyy}" +
        (IsClosed ? $" - RETURNED {ReturnedDate:MMM d}" : DaysUntilReturn < 0 ? $" - OVERDUE by {-DaysUntilReturn} days" : $" - in {DaysUntilReturn} days") +
        $" - condition out: {(ConditionOutLogged ? "logged" : "NOT logged")} / in: {(ConditionInLogged ? "logged" : "pending")}";
}
