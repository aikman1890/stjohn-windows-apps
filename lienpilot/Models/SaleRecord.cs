namespace LienPilot.Models;

public sealed class SaleRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UnitId { get; set; } = string.Empty;
    public string UnitLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public decimal SalePrice { get; set; }
    public decimal AppliedToClaim { get; set; }
    public decimal Excess => Math.Max(0m, SalePrice - AppliedToClaim);
    public DateTimeOffset? ExcessNoticeDate { get; set; }
    public DateTimeOffset? ExcessClaimedDate { get; set; }
    public DateTimeOffset? ExcessDisposedDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {UnitLabel} - sold {SalePrice:C}, applied {AppliedToClaim:C}, excess {Excess:C}" +
        (ExcessClaimedDate is { } c ? $" - excess claimed {c:MMM d}" : Excess > 0 ? " - excess UNCLAIMED" : string.Empty);
}
