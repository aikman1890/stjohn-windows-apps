namespace TruckPilot.Models;

public sealed class OperatorProfile
{
    public string LegalName { get; set; } = string.Empty;
    public string Dba { get; set; } = string.Empty;
    public string IftaAccount { get; set; } = string.Empty;
    public string BaseJurisdiction { get; set; } = string.Empty;
    public string UnitId { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string FuelType { get; set; } = "Diesel";
    public string Notes { get; set; } = string.Empty;
}
