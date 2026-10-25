using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ShopPilot.Models;

public sealed class ShopJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string JobNumber { get; set; } = string.Empty;
    public string QuoteId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerLabel { get; set; } = string.Empty;
    public string PartDescription { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string MaterialCertRef { get; set; } = string.Empty;
    public decimal MaterialCost { get; set; }
    public int Quantity { get; set; } = 1;
    public int QtyShipped { get; set; }
    public decimal QuotedPrice { get; set; }
    public DateTimeOffset DueDate { get; set; } = DateTimeOffset.Now.AddDays(7);
    public string CustomerPO { get; set; } = string.Empty;
    public string WorkCenter { get; set; } = string.Empty;
    public int Priority { get; set; } = 5;
    public string Status { get; set; } = "Open"; // Open / In process / Complete / Shipped / Closed
    public DateTimeOffset? ShipDate { get; set; }
    public string InvoiceRef { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public int DaysToDue => (DueDate.Date - DateTimeOffset.Now.Date).Days;

    public Brush DueBrush
    {
        get
        {
            if (Status is "Shipped" or "Closed") return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            if (DaysToDue < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (DaysToDue <= 2) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{JobNumber} - {CustomerLabel} - {PartDescription} - qty {Quantity} - due {DueDate:MMM d, yyyy} ({DaysToDue}d) - {Status}";
}
