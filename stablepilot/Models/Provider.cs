namespace StablePilot.Models;

public sealed class Provider
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    /// <summary>Vet / Farrier / Dentist / Chiropractor / Transporter / Other.</summary>
    public string Trade { get; set; } = "Vet";
    public string Contact { get; set; } = string.Empty;
    public string HorsesNote { get; set; } = string.Empty;
    public string LastVisitNote { get; set; } = string.Empty;
    /// <summary>Next scheduled round - a farrier day is booked against the cycle clocks.</summary>
    public string NextRoundNote { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Name} - {Trade} - {Contact} - next round: {NextRoundNote}";
}
