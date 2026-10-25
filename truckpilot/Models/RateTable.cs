namespace TruckPilot.Models;

/// <summary>
/// One quarter's tax rates, entered BY THE OPERATOR from the official IFTA
/// rate chart and explicitly confirmed. TruckPilot ships with NO rates.
/// </summary>
public sealed class RateTable
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int Year { get; set; }
    public int Quarter { get; set; } = 1; // 1-4
    public string Jurisdiction { get; set; } = string.Empty;
    public decimal RatePerGallon { get; set; }
    public bool Confirmed { get; set; }
    public DateTimeOffset ConfirmedDate { get; set; }
    public string Source { get; set; } = string.Empty;

    public string Key => $"{Year}-Q{Quarter}-{Jurisdiction.Trim().ToUpperInvariant()}";

    public string DisplayLine =>
        $"{Year} Q{Quarter} - {Jurisdiction} - {RatePerGallon:C4}/gal" +
        (Confirmed ? $" - confirmed {ConfirmedDate:MMM d, yyyy}" : " - NOT confirmed") +
        (string.IsNullOrWhiteSpace(Source) ? string.Empty : $" - {Source}");
}
