using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AgreementPilot.Models;

public sealed class Agreement
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerLabel { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset RenewalDate { get; set; } = DateTimeOffset.Now.AddYears(1);
    public int VisitsIncluded { get; set; } = 2;
    public int VisitsDelivered { get; set; }
    public string CoveredNote { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active / Renewal due / Renewed / Lapsed
    public decimal DiscountsHonored { get; set; }
    public string PaymentStatus { get; set; } = "Paid";
    public string Notes { get; set; } = string.Empty;

    public int VisitsOwed => Math.Max(0, VisitsIncluded - VisitsDelivered);
    public int DaysToRenewal => (RenewalDate.Date - DateTimeOffset.Now.Date).Days;

    public string DisplayLine =>
        $"{PlanName} - {CustomerLabel} - {Price:C} - renews {RenewalDate:MMM d, yyyy} ({DaysToRenewal} days) - {Status} - visits {VisitsDelivered}/{VisitsIncluded}";

    public Brush RenewalBrush
    {
        get
        {
            if (Status == "Lapsed") return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            if (DaysToRenewal < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (DaysToRenewal <= 30) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }
}
