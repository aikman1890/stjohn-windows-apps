namespace KennelPilot.Models;

public sealed class CareEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StayId { get; set; } = string.Empty;
    public string StayLabel { get; set; } = string.Empty;
    public DateTimeOffset When { get; set; } = DateTimeOffset.Now;
    public string Kind { get; set; } = "Feeding";
    public string Detail { get; set; } = string.Empty;
    public string StaffInitials { get; set; } = string.Empty;

    public string DisplayLine => $"{When:MMM d, h:mm tt} - {Kind} - {Detail} [{StaffInitials}]";
}
