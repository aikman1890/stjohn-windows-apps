namespace VenuePilot.Models;

public sealed class VenueSettings
{
    public string VenueName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public List<string> Spaces { get; set; } = new() { "Barn", "Hall", "Grounds" };
    /// <summary>Rate card lines: "Package name|Price|Season note".</summary>
    public List<string> Packages { get; set; } = new()
    {
        "Full Day - Peak|4500|May-Oct Saturdays",
        "Full Day - Off-peak|3200|Nov-Apr",
        "Half Day / Sunday|2200|Any season",
    };
    public List<string> EventTypes { get; set; } = new()
    {
        "Wedding", "Reception only", "Reunion", "Corporate", "Birthday", "Quinceanera", "Other",
    };
}
