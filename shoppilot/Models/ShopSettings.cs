namespace ShopPilot.Models;

public sealed class ShopSettings
{
    public string ShopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    /// <summary>Shop labor rate used to turn traveler hours into labor cost.</summary>
    public decimal LaborRate { get; set; } = 85m;
    public List<string> WorkCenters { get; set; } = new() { "Saw", "Mill", "Lathe", "Weld", "Inspection", "Shipping" };
    /// <summary>Operation library as "Name|WorkCenter" lines.</summary>
    public List<string> OperationLibrary { get; set; } = new()
    {
        "Saw cut|Saw",
        "Mill - face & profile|Mill",
        "Mill - drill/tap|Mill",
        "Lathe - turn|Lathe",
        "Lathe - bore|Lathe",
        "Weld - TIG|Weld",
        "Deburr|Inspection",
        "Inspect & measure|Inspection",
        "Pack & ship|Shipping",
    };
}
