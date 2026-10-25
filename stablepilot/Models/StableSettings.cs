namespace StablePilot.Models;

public sealed class StableSettings
{
    public string BarnName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "USD";
}
