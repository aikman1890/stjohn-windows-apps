namespace HealthPilot.Models;

/// <summary>One medical appointment.</summary>
public sealed class AppointmentItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTimeOffset When { get; set; } = DateTimeOffset.Now;
    public string Location { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Title} - {When:MMM d, yyyy h:mm tt}";
    public string SubLine =>
        (string.IsNullOrWhiteSpace(Provider) ? "" : Provider) +
        (string.IsNullOrWhiteSpace(Location) ? "" : $" - {Location}") +
        (string.IsNullOrWhiteSpace(Notes) ? "" : $" - {Notes}");
}
