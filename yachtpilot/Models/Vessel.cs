namespace YachtPilot.Models;

public sealed class Vessel
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Flag { get; set; } = string.Empty;
    public string Berth { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerContact { get; set; } = string.Empty;
    public string Dimensions { get; set; } = string.Empty;
    public DateTimeOffset? InsuranceExpiry { get; set; }
    public DateTimeOffset? RegistrationExpiry { get; set; }
    /// <summary>Engine hours as last logged by the manager - drives metered maintenance clocks.</summary>
    public decimal CurrentEngineHours { get; set; }
    /// <summary>Charter-ready override: the manager's dated note accepting a not-ready state. Never silent.</summary>
    public string ReadyOverrideNote { get; set; } = string.Empty;
    public DateTimeOffset? ReadyOverrideDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}
