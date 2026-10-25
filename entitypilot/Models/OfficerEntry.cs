namespace EntityPilot.Models;

public sealed class OfficerEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string EntityId { get; set; } = string.Empty;
    public string EntityLabel { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTimeOffset AppointedDate { get; set; } = DateTimeOffset.Now;

    public string DisplayLine => $"{Name} - {Role} - appointed {AppointedDate:MMM d, yyyy} - {EntityLabel}";
}
