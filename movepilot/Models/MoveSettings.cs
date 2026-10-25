namespace MovePilot.Models;

public sealed class MoveSettings
{
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal DefaultHourlyRate { get; set; } = 150m;
    public int DefaultCrewSize { get; set; } = 3;
    /// <summary>Acknowledge-clock days copied from the mover's OWN claim paperwork. User-confirmed; the app ships no law.</summary>
    public int DefaultClaimAckDays { get; set; } = 30;
    /// <summary>Inventory presets as "Name|cubic feet|pounds" lines - the mover's own figures, editable.</summary>
    public List<string> ItemPresets { get; set; } = new()
    {
        "Sofa (3-seat)|60|150",
        "Armchair|25|60",
        "Coffee table|10|40",
        "Dining table|30|90",
        "Dining chair|5|15",
        "Queen bed set|65|180",
        "Dresser|30|120",
        "Wardrobe box|10|40",
        "Bookcase|20|70",
        "Desk|25|80",
        "Washer|25|180",
        "Dryer|25|130",
        "Refrigerator|40|250",
        "Piano (upright)|55|500",
        "Safe|15|400",
        "Pool table|50|650",
        "Box (medium)|3|30",
        "Box (large)|6|45",
        "TV (large)|8|45",
        "Rug|8|40",
    };
    public List<string> TermsPresets { get; set; } = new()
    {
        "Deposit due at booking; balance on delivery before unloading.",
        "Hourly billing starts at arrival, minimum 2 hours.",
        "Not-to-exceed: the final charge will not exceed this estimate for the inventoried items listed.",
    };
}
