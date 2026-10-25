namespace DenialPilot.Models;

/// <summary>
/// One entry in a case's proof-of-sending ledger: what happened, when, by
/// what method, and any tracking or confirmation number. This is the
/// chronological record that wins escalations.
/// </summary>
public sealed class LedgerEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CaseId { get; set; }
    public string CaseLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Kind { get; set; } = "Note";
    public string Detail { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Tracking { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Kind}";

    public string SubLine =>
        CaseLabel +
        (string.IsNullOrWhiteSpace(Detail) ? "" : $" - {Detail}") +
        (string.IsNullOrWhiteSpace(Method) ? "" : $" - via {Method}") +
        (string.IsNullOrWhiteSpace(Tracking) ? "" : $" - {Tracking}");
}
