namespace WheelPilot.Models;

public sealed class MaintenanceEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CarId { get; set; } = string.Empty;
    public string CarLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Work { get; set; } = string.Empty;
    public decimal Cost { get; set; }

    public string DisplayLine => $"{Date:MMM d, yyyy} - {CarLabel} - {Work} - {Cost:C}";
}
