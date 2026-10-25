namespace CharterPilot.Models;

public sealed class CharterSettings
{
    public string BrokerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "USD";
    public List<string> AircraftClasses { get; set; } = new()
    {
        "Turboprop", "Light jet", "Midsize jet", "Super-midsize", "Heavy jet", "Ultra long range", "Helicopter",
    };
}
