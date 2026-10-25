namespace FarewellPilot.Models;

public sealed class AftercareTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; set; } = string.Empty;
    public string CaseLabel { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty; // 1 week / 1 month / Anniversary / Custom
    public DateTimeOffset DueDate { get; set; } = DateTimeOffset.Now;
    public bool Done { get; set; }
    public DateTimeOffset? DoneDate { get; set; }
    public string Note { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{(Done ? "[done] " : "")}{Kind} - {CaseLabel} - due {DueDate:MMM d, yyyy}" +
        (!Done && DueDate.Date < DateTimeOffset.Now.Date ? " - OVERDUE" : string.Empty);
}
