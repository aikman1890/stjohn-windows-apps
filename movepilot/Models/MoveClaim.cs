using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MovePilot.Models;

public sealed class MoveClaim
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string MoveId { get; set; } = string.Empty;
    public string CustomerLabel { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    public decimal AmountClaimed { get; set; }
    public DateTimeOffset FiledDate { get; set; } = DateTimeOffset.Now;
    /// <summary>Days the mover's own claim paperwork allows to acknowledge - user-confirmed, ships no law.</summary>
    public int AckClockDays { get; set; } = 30;
    public DateTimeOffset? AckDate { get; set; }
    public DateTimeOffset? InspectDate { get; set; }
    public DateTimeOffset? OfferDate { get; set; }
    public decimal OfferAmount { get; set; }
    public DateTimeOffset? SettledDate { get; set; }
    public decimal SettledAmount { get; set; }
    public string Status { get; set; } = "Open"; // Open / Acknowledged / Inspected / Offered / Settled / Declined
    public string ProofLog { get; set; } = string.Empty;

    public DateTimeOffset AckDue => FiledDate.AddDays(AckClockDays);
    public int DaysToAck => (AckDue.Date - DateTimeOffset.Now.Date).Days;
    public bool AckOverdue => AckDate is null && Status is not "Settled" and not "Declined" && DaysToAck < 0;

    public Brush ClockBrush
    {
        get
        {
            if (AckDate is not null || Status is "Settled" or "Declined")
                return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
            if (DaysToAck < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (DaysToAck <= 5) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{CustomerLabel} - {Item} - claimed {AmountClaimed:C} - filed {FiledDate:MMM d, yyyy} - {Status}" +
        (AckDate is null && Status is not "Settled" and not "Declined"
            ? $" - acknowledge by {AckDue:MMM d, yyyy} ({DaysToAck} days)"
            : string.Empty);
}
