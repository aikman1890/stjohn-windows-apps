namespace VenuePilot.Models;

public sealed class VendorEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string EventId { get; set; } = string.Empty;
    public string Role { get; set; } = "Caterer";
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Role}: {Name} - {Phone} - arrives {ArrivalTime}";
}
