namespace KennelPilot.Models;

public sealed class Owner
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AltPhone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string VetName { get; set; } = string.Empty;
    public string VetPhone { get; set; } = string.Empty;
    public string PaymentNote { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
