namespace ShopPilot.Models;

public sealed class ShopQuote
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Number { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerLabel { get; set; } = string.Empty;
    public string PartDescription { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string QtyBreaks { get; set; } = string.Empty;
    public decimal QuotedPrice { get; set; }
    public int LeadTimeDays { get; set; } = 10;
    public decimal EstMaterialCost { get; set; }
    public decimal EstLaborHours { get; set; }
    public decimal EstOutsideCost { get; set; }
    public string Status { get; set; } = "Open"; // Open / Won / Lost / No-bid
    public string LossReason { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

    public decimal UnitPrice => Quantity > 0 ? QuotedPrice / Quantity : 0m;

    public string DisplayLine =>
        $"{Number} - {CustomerLabel} - {PartDescription} - qty {Quantity} - {QuotedPrice:C} - {Status}" +
        (Status == "Lost" && !string.IsNullOrWhiteSpace(LossReason) ? $" ({LossReason})" : string.Empty) +
        $" - {Date:MMM d, yyyy}";
}
