namespace BayPilot.Models;

/// <summary>
/// One vehicle belonging to a customer. VIN is a stored string only -
/// BayPilot never decodes or fetches vehicle data. Service interval
/// fields are the shop's own estimates, labeled as such.
/// </summary>
public sealed class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public string CustomerLabel { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public int ServiceIntervalMonths { get; set; }
    public int ServiceIntervalMiles { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Year} {Make} {Model}".Trim();

    public string FullLabel => $"{CustomerLabel} - {DisplayLine}";

    public string SubLine =>
        (string.IsNullOrWhiteSpace(Plate) ? "" : $"Plate {Plate} - ") +
        $"{Mileage:N0} mi" +
        (string.IsNullOrWhiteSpace(Vin) ? "" : $" - VIN {Vin}");
}
