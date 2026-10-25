using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CharterPilot.Models;

public sealed class Quote
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ClientId { get; set; } = string.Empty;
    public string ClientLabel { get; set; } = string.Empty;
    public string FromText { get; set; } = string.Empty;
    public string ToText { get; set; } = string.Empty;
    public DateTimeOffset DepartDate { get; set; } = DateTimeOffset.Now.AddDays(3);
    public string AircraftClass { get; set; } = string.Empty;
    public string OperatorId { get; set; } = string.Empty;
    public string OperatorLabel { get; set; } = string.Empty;
    public string TailLabel { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal PriceQuoted { get; set; }
    public string Currency { get; set; } = "USD";
    /// <summary>Sent / Follow-up due / Negotiating / Booked / Lost / Expired.</summary>
    public string Status { get; set; } = "Sent";
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset LastTouchedAt { get; set; } = DateTimeOffset.Now;
    /// <summary>Follow-up preset in hours: 4 / same-day (8) / next morning (20) / custom.</summary>
    public decimal FollowUpHours { get; set; } = 4m;
    /// <summary>Price / Timing / No response / Aircraft unavailable / Another broker / (blank while live).</summary>
    public string LostReason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    /// <summary>Margin is computed, never typed - the ledger cannot disagree with itself.</summary>
    public decimal Margin => PriceQuoted - Cost;

    public bool IsLive => Status is "Sent" or "Follow-up due" or "Negotiating";
    public DateTimeOffset FollowUpDue => LastTouchedAt.AddHours((double)FollowUpHours);
    public bool FollowUpOverdue => IsLive && DateTimeOffset.Now > FollowUpDue;
    public double HoursUntilFollowUp => (FollowUpDue - DateTimeOffset.Now).TotalHours;

    public Brush FlagBrush
    {
        get
        {
            if (!IsLive) return new SolidColorBrush(Color.FromArgb(255, 140, 140, 140));
            if (FollowUpOverdue) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (HoursUntilFollowUp <= 2) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{ClientLabel} - {FromText} -> {ToText} - {DepartDate:MMM d} - {AircraftClass} - quoted {PriceQuoted:0.00} {Currency} (margin {Margin:0.00}) - {Status}" +
        (IsLive ? (FollowUpOverdue ? $" - FOLLOW-UP OVERDUE by {-HoursUntilFollowUp:0.0}h" : $" - follow up in {HoursUntilFollowUp:0.0}h") : string.Empty);
}
