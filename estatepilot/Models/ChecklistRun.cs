namespace EstatePilot.Models;

/// <summary>A dated completion record for a reusable checklist - the proof ledger for the invisible work.</summary>
public sealed class ChecklistRun
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PropertyId { get; set; } = string.Empty;
    public string PropertyLabel { get; set; } = string.Empty;
    public string ChecklistName { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public int DoneCount { get; set; }
    public int TotalCount { get; set; }
    public string MissedItems { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public bool Complete => TotalCount > 0 && DoneCount >= TotalCount;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {PropertyLabel} - {ChecklistName} - {DoneCount}/{TotalCount} items" +
        (Complete ? " - COMPLETE" : $" - MISSED: {MissedItems}");
}
