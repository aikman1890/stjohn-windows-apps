namespace TruckPilot.Models;

public sealed class FuelPurchase
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Jurisdiction { get; set; } = string.Empty;
    public decimal Gallons { get; set; }
    public decimal TotalCost { get; set; }
    public string ReceiptRef { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {Jurisdiction} - {Gallons:N3} gal - {TotalCost:C}" +
        (string.IsNullOrWhiteSpace(ReceiptRef) ? string.Empty : $" - receipt {ReceiptRef}");
}
