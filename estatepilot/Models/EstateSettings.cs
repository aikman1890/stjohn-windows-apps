namespace EstatePilot.Models;

public sealed class EstateSettings
{
    public string ManagerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    /// <summary>Checklist templates: "Name|item; item; item".</summary>
    public List<string> ChecklistTemplates { get; set; } = new()
    {
        "Open the house|Water on and pressure checked; HVAC set to arrival temperature; Generator test run logged; Pool system started; Pantry and fridge stocked per standing list; Fresh flowers and linens placed; Vehicles fuelled and checked",
        "Close the house|Water shut off at main; HVAC set to vacant mode; Pool closed and covered; Perishables cleared; Windows and shutters secured; Mail held; Final walk-through photographed",
        "Guest arrival|Guest rooms set per preference notes; Bath amenities placed; Welcome items per standing instructions; Guest wifi note card placed (network name only - credentials stay with the household); Itinerary printed and placed",
    };
}
