namespace WheelPilot.Models;

public sealed class Instructor
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string LicenseNote { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public string Notes { get; set; } = string.Empty;
}
