namespace GuardPilot.Models;

/// <summary>
/// Post orders are versioned like WellPilot's decks: a new revision supersedes
/// the old one, and the version in force on any past night stays retrievable.
/// </summary>
public sealed class PostOrderRevision
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PostId { get; set; } = string.Empty;
    public string PostLabel { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public DateTimeOffset EffectiveDate { get; set; } = DateTimeOffset.Now;
    public string OrdersText { get; set; } = string.Empty;
    public string SiteInstructions { get; set; } = string.Empty;
    public string EmergencyContacts { get; set; } = string.Empty;
    public string EscalationChain { get; set; } = string.Empty;
    public bool Active { get; set; } = true;

    public string DisplayLine =>
        $"v{Version} - effective {EffectiveDate:MMM d, yyyy}" + (Active ? " - IN FORCE" : " - superseded");
}
