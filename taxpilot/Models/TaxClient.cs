namespace TaxPilot.Models;

public sealed class TaxClient
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "Individual"; // Individual / Business
    public string EntityType { get; set; } = "1040";
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    /// <summary>Carry-forward flags for next season (K-1 expected, rental sold, new baby - free text).</summary>
    public string CarryNotes { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
