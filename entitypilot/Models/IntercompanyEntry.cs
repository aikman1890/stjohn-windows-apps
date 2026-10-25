namespace EntityPilot.Models;

public sealed class IntercompanyEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FromEntityId { get; set; } = string.Empty;
    public string FromLabel { get; set; } = string.Empty;
    public string ToEntityId { get; set; } = string.Empty;
    public string ToLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    /// <summary>Loan / Transfer / Repayment (a repayment runs back from borrower to lender - enter From = borrower).</summary>
    public string Kind { get; set; } = "Loan";
    public decimal Amount { get; set; }
    public string TermsNote { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Kind} {Amount:C} - {FromLabel} -> {ToLabel} - {TermsNote}";
}
