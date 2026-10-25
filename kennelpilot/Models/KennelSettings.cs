namespace KennelPilot.Models;

public sealed class KennelSettings
{
    public string KennelName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal NightlyRate { get; set; } = 45m;
    public decimal SecondPetRate { get; set; } = 30m;
    public decimal BathRate { get; set; } = 25m;
    public decimal MedicationRatePerDay { get; set; } = 5m;
    public int Capacity { get; set; } = 20;
}
