using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace VenuePilot.Models;

public sealed class VenueEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ClientNames { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Space { get; set; } = string.Empty;
    public string EventType { get; set; } = "Wedding";
    public DateTimeOffset EventDate { get; set; } = DateTimeOffset.Now.AddDays(60);
    public string Status { get; set; } = "Inquiry"; // Inquiry / Hold / Contracted / Completed / Cancelled
    public DateTimeOffset? HoldExpiry { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public decimal PackagePrice { get; set; }
    public int GuestCount { get; set; }
    public DateTimeOffset? ContractDate { get; set; }
    public DateTimeOffset? BalanceDueDate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTimeOffset? TourDate { get; set; }
    public DateTimeOffset? LastContacted { get; set; }
    public string LoadIn { get; set; } = string.Empty;
    public string LoadOut { get; set; } = string.Empty;
    public string TimelineNotes { get; set; } = string.Empty;
    public string CancelReason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public bool HoldExpired => Status == "Hold" && HoldExpiry is { } h && h.Date < DateTimeOffset.Now.Date;
    public bool InquiryStale => Status == "Inquiry" && LastContacted is { } l &&
        (DateTimeOffset.Now - l).TotalDays > 7;

    public Brush StatusBrush
    {
        get
        {
            if (HoldExpired || InquiryStale) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            return Status switch
            {
                "Contracted" => new SolidColorBrush(Color.FromArgb(255, 60, 160, 80)),
                "Hold" => new SolidColorBrush(Color.FromArgb(255, 220, 140, 30)),
                "Completed" => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)),
                "Cancelled" => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)),
                _ => new SolidColorBrush(Color.FromArgb(255, 90, 130, 200)),
            };
        }
    }

    public string DisplayLine =>
        $"{EventDate:ddd, MMM d, yyyy} - {Space} - {ClientNames} - {EventType} - {Status}" +
        (HoldExpired ? " - HOLD EXPIRED" : string.Empty) +
        (InquiryStale ? " - INQUIRY STALE" : string.Empty);
}
