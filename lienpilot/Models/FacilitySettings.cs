namespace LienPilot.Models;

public sealed class FacilitySettings
{
    public string FacilityName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal LateFee { get; set; } = 20m;
    public decimal LienFee { get; set; } = 50m;
    public decimal SaleFee { get; set; } = 75m;
    public decimal LockCutFee { get; set; } = 25m;
    public string ActiveProfileId { get; set; } = string.Empty;
}
