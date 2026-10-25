namespace KennelPilot.Models;

public sealed class KennelInvoiceLine
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public decimal Rate { get; set; }
    public decimal Amount => Math.Round(Quantity * Rate, 2);
}

public sealed class KennelInvoice
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string InvoiceNumber { get; set; } = string.Empty;
    public string StayId { get; set; } = string.Empty;
    public string PetLabel { get; set; } = string.Empty;
    public string OwnerLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public List<KennelInvoiceLine> Lines { get; set; } = new();
    public decimal Total => Lines.Sum(l => l.Amount);
    public string PayStatus { get; set; } = "Unpaid";
    public decimal AmountPaid { get; set; }
    public decimal Balance => Total - AmountPaid;

    public string DisplayLine => $"{InvoiceNumber} - {PetLabel} - {Total:C} - {PayStatus}";
    public string SubLine => $"{Date:MMM d, yyyy} - {OwnerLabel} - balance {Balance:C}";
}
