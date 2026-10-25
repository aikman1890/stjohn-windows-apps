namespace PlatePilot.Models;

public sealed class PlateSettings
{
    public string RestaurantName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal DefaultTargetPercent { get; set; } = 30m;
}
