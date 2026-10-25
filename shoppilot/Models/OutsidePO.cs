using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ShopPilot.Models;

public sealed class OutsidePO
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string JobId { get; set; } = string.Empty;
    public string JobLabel { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string PONumber { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public DateTimeOffset SentDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset PromisedDate { get; set; } = DateTimeOffset.Now.AddDays(5);
    public DateTimeOffset? ReturnedDate { get; set; }
    public string CertRef { get; set; } = string.Empty;

    public bool Overdue => ReturnedDate is null && PromisedDate.Date < DateTimeOffset.Now.Date;

    public Brush StatusBrush => Overdue
        ? new SolidColorBrush(Color.FromArgb(255, 200, 60, 60))
        : new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));

    public string DisplayLine =>
        $"PO {PONumber} - {Vendor} - {Service} - {Cost:C} - promised {PromisedDate:MMM d, yyyy}" +
        (ReturnedDate is { } r ? $" - returned {r:MMM d, yyyy}" : Overdue ? " - OVERDUE" : " - out") +
        $" - {JobLabel}";
}
