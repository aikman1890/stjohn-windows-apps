namespace BayPilot.Models;

/// <summary>
/// A snapshot line on an issued invoice (copied from the RO's approved
/// lines at invoicing time, adjustable before the invoice is saved).
/// </summary>
public sealed class ShopInvoiceLine
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public decimal Rate { get; set; }

    public decimal Amount => Quantity * Rate;

    public string DisplayLine => $"{Description} - {Quantity:0.##} x {Rate:C} = {Amount:C}";
}

/// <summary>
/// A simple invoice created from a repair order. BayPilot records
/// payment; it never processes it - no card data ever touches the app.
/// </summary>
public sealed class ShopInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid RoId { get; set; }
    public string RoLabel { get; set; } = string.Empty;
    public string CustomerLabel { get; set; } = string.Empty;
    public string VehicleLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public List<ShopInvoiceLine> Lines { get; set; } = new();
    public decimal SuppliesFee { get; set; }
    public decimal TaxPercent { get; set; }
    public string PayStatus { get; set; } = "Unpaid";
    public decimal AmountPaid { get; set; }

    public decimal Subtotal => Lines.Sum(l => l.Amount);

    public decimal TaxAmount => (Subtotal + SuppliesFee) * TaxPercent / 100m;

    public decimal Total => Subtotal + SuppliesFee + TaxAmount;

    public decimal Balance => Total - AmountPaid;

    public string DisplayLine => $"{InvoiceNumber} - {CustomerLabel}";

    public string SubLine => $"{Date:MMM d, yyyy} - total {Total:C} - {PayStatus} (paid {AmountPaid:C})";
}
