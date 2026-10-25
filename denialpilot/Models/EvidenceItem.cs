namespace DenialPilot.Models;

/// <summary>
/// One item on a case's evidence checklist: gathered or not, with notes.
/// </summary>
public sealed class EvidenceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CaseId { get; set; }
    public string CaseLabel { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => ItemName;
}
