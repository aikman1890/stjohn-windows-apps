namespace YachtPilot.Models;

public sealed class CrewMember
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    /// <summary>Vessel this crew member is currently assigned to ("" = unassigned).</summary>
    public string VesselId { get; set; } = string.Empty;
    public string VesselLabel { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public string Notes { get; set; } = string.Empty;
}
