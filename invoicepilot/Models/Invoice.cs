namespace InvoicePilot.Models;

/// <summary>
/// One invoice line: description, quantity, and rate. The amount is derived.
/// </summary>
public sealed class InvoiceLineItem
{
    public string Description { get; set; } = string.Empty;
    public double Quantity { get; set; } = 1;
    public double Rate { get; set; }

    public double Amount => Quantity * Rate;

    public string DisplayLine => $"{Description} - {Quantity:0.##} x {Rate:C} = {Amount:C}";
}

/// <summary>
/// One BCCS invoice or quote: client, dates, status, line items, and tax.
/// Totals are derived from the line items.
/// </summary>
public sealed class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientContact { get; set; } = string.Empty;
    public DateTimeOffset IssueDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? DueDate { get; set; }
    public string Status { get; set; } = "Draft";
    public double TaxPercent { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<InvoiceLineItem> LineItems { get; set; } = new();

    public double Subtotal => LineItems.Sum(i => i.Amount);

    public double TaxAmount => Subtotal * TaxPercent / 100.0;

    public double Total => Subtotal + TaxAmount;

    public string DisplayLine => $"{InvoiceNumber} - {ClientName}";

    public string SubLine => $"{Status} - issued {IssueDate:MMM d, yyyy} - total {Total:C}";
}
