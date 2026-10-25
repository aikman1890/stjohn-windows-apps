namespace EntityPilot.Models;

public sealed class Obligation
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string EntityId { get; set; } = string.Empty;
    public string EntityLabel { get; set; } = string.Empty;
    public string Kind { get; set; } = "Annual report"; // Annual report / Franchise tax / Agent renewal / DBA renewal / One-off / Other
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset DueDate { get; set; } = DateTimeOffset.Now;
    public bool RecursYearly { get; set; } = true;
    public DateTimeOffset? LastDoneDate { get; set; }
    public string Note { get; set; } = string.Empty;

    public int DaysUntil => (DueDate.Date - DateTimeOffset.Now.Date).Days;

    public string DisplayLine =>
        $"{DueDate:MMM d, yyyy} ({(DaysUntil < 0 ? $"{-DaysUntil} days OVERDUE" : DaysUntil == 0 ? "DUE TODAY" : $"in {DaysUntil} days")}) - {EntityLabel} - {Title} [{Kind}]";
}
