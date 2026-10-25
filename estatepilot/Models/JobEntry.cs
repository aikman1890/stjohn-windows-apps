namespace EstatePilot.Models;

/// <summary>Work done at a property - the record that answers "when was the roof last done and what did it cost?" Costs only; no financial management.</summary>
public sealed class JobEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PropertyId { get; set; } = string.Empty;
    public string PropertyLabel { get; set; } = string.Empty;
    public string VendorLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Work { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string Currency { get; set; } = "USD";
    public string InvoiceRef { get; set; } = string.Empty;
    public DateTimeOffset? WarrantyExpiry { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {PropertyLabel} - {Work} - {VendorLabel} - {Cost:0.00} {Currency} (inv. {InvoiceRef})" +
        (WarrantyExpiry is { } we ? $" - warranty to {we:MMM d, yyyy}" : string.Empty);
}
