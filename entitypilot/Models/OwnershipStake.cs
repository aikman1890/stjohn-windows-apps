namespace EntityPilot.Models;

/// <summary>Versioned like a division order deck: changes supersede, never rewrite.</summary>
public sealed class OwnershipStake
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string EntityId { get; set; } = string.Empty;
    public string EntityLabel { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Class { get; set; } = "Membership";
    public decimal Percent { get; set; }
    public decimal Contribution { get; set; }
    public DateTimeOffset EffectiveDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? SupersededDate { get; set; }
    public string Note { get; set; } = string.Empty;

    public bool IsActive => SupersededDate is null;

    public string DisplayLine =>
        $"{OwnerName} - {Percent:0.00}% ({Class}) - contributed {Contribution:C} - eff. {EffectiveDate:MMM d, yyyy}" +
        (IsActive ? string.Empty : $" - superseded {SupersededDate:MMM d, yyyy}");
}
