namespace BayPilot.Models;

/// <summary>
/// One entry in an RO's approval log: what was decided, when, by what
/// method, and who approved. This is the shop's dispute record.
/// </summary>
public sealed class ApprovalEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoId { get; set; }
    public string RoLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string LineDescription { get; set; } = string.Empty;
    public string Decision { get; set; } = "Approved";
    public string Method { get; set; } = "In person";
    public string ApprovedBy { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy h:mm tt} - {Decision}";

    public string SubLine =>
        $"{RoLabel} - {LineDescription}" +
        (string.IsNullOrWhiteSpace(Method) ? "" : $" - {Method}") +
        (string.IsNullOrWhiteSpace(ApprovedBy) ? "" : $" - by {ApprovedBy}");
}
