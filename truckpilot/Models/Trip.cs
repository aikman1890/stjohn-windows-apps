namespace TruckPilot.Models;

public sealed class Trip
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Jurisdiction { get; set; } = string.Empty;
    public string RouteNotes { get; set; } = string.Empty;
    public decimal OdometerStart { get; set; }
    public decimal OdometerEnd { get; set; }
    public decimal Miles => OdometerEnd - OdometerStart;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {Jurisdiction} - {Miles:N1} mi (odo {OdometerStart:N0} to {OdometerEnd:N0})";
}
