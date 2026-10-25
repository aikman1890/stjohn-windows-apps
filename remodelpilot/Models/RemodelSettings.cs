namespace RemodelPilot.Models;

public sealed class RemodelSettings
{
    public string BusinessName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal DefaultMarkupPercent { get; set; } = 20m;
    public List<string> Categories { get; set; } = new()
    {
        "Demo", "Framing", "Electrical", "Plumbing", "HVAC", "Drywall",
        "Paint", "Flooring", "Cabinets", "Countertops", "Tile", "Fixtures",
        "Permits", "Disposal", "Labor", "Other",
    };
}
