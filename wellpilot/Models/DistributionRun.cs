namespace WellPilot.Models;

public sealed class DistributionRun
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string WellId { get; set; } = string.Empty;
    public string WellLabel { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public DateTimeOffset RunDate { get; set; } = DateTimeOffset.Now;
    public decimal NetRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    /// <summary>Snapshot note: which deck (versions + as-of date) this run used. Immutable once run.</summary>
    public string DeckSnapshot { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Month} - {WellLabel} - net revenue {NetRevenue:C} - expenses {TotalExpenses:C} - run {RunDate:MMM d, yyyy}";
}

/// <summary>One owner's statement line in a run: revenue credit, JIB charge, net.</summary>
public sealed class StatementLine
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string RunId { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerLabel { get; set; } = string.Empty;
    public string InterestType { get; set; } = string.Empty;
    /// <summary>The deck decimal actually used, snapshotted at run time.</summary>
    public decimal DecimalUsed { get; set; }
    public decimal RevenueCredit { get; set; }
    public decimal JibCharge { get; set; }
    public decimal NetAmount => RevenueCredit - JibCharge;

    public string DisplayLine =>
        $"{OwnerLabel} ({InterestType} {DecimalUsed:0.00000000}) - revenue {RevenueCredit:C} - JIB {JibCharge:C} - net {NetAmount:C}";
}
