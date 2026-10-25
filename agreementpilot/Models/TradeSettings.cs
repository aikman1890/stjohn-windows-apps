namespace AgreementPilot.Models;

public sealed class TradeSettings
{
    public string ShopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    /// <summary>Days before renewal when it turns orange on the pipeline. User-set.</summary>
    public int RenewalOrangeDays { get; set; } = 30;
    /// <summary>Equipment age (years) that lands a unit on the replacement watch. User-set.</summary>
    public int WatchAgeYears { get; set; } = 15;
    public List<string> PlanNames { get; set; } = new() { "Comfort Plan", "Comfort Plan Plus" };
    public List<string> EquipmentTypes { get; set; } = new()
    {
        "Furnace", "Air conditioner", "Heat pump", "Water heater", "Boiler",
        "Electrical panel", "Generator", "Ductless mini-split", "Other",
    };
}
