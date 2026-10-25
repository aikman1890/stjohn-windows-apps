namespace RemodelPilot.Models;

public sealed class Draw
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string JobId { get; set; } = string.Empty;
    public string JobLabel { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TriggerNote { get; set; } = string.Empty;
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? InvoicedDate { get; set; }
    public DateTimeOffset? PaidDate { get; set; }

    public string State => PaidDate is not null ? "Paid" : InvoicedDate is not null ? "Invoiced" : "Not invoiced";
    public bool OverdueToInvoice =>
        InvoicedDate is null && DueDate is { } d && d.Date < DateTimeOffset.Now.Date;

    public string DisplayLine =>
        $"{Label} - {JobLabel} - {Amount:C} - {State}" +
        (DueDate is { } dd ? $" - due {dd:MMM d, yyyy}" : string.Empty) +
        (OverdueToInvoice ? " - OVERDUE TO INVOICE" : string.Empty);
}
