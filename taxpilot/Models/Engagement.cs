using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace TaxPilot.Models;

public sealed class Engagement
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ClientId { get; set; } = string.Empty;
    public string ClientLabel { get; set; } = string.Empty;
    public int TaxYear { get; set; } = DateTime.Now.Year - 1;
    public string EntityType { get; set; } = "1040";
    public string Status { get; set; } = "Not started";
    // Not started / Docs pending / In prep / In review / Ready to file / Filed / Accepted / Rejected-fix / Extended / Done
    public decimal FeeQuoted { get; set; }
    public decimal FeePaid { get; set; }
    public bool Extended { get; set; }
    public DateTimeOffset? ExtensionFiledDate { get; set; }
    /// <summary>Preparer's own per-client due date when it differs from the entity table.</summary>
    public DateTimeOffset? PersonalDueDate { get; set; }
    public DateTimeOffset? LastContacted { get; set; }
    public string Notes { get; set; } = string.Empty;

    public decimal FeeOwed => FeeQuoted - FeePaid;

    /// <summary>Set by the page layer from the settings table before display.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public DateTimeOffset? ComputedDue { get; set; }

    public int? DaysToDue => ComputedDue is { } d ? (d.Date - DateTimeOffset.Now.Date).Days : null;

    public Brush DueBrush
    {
        get
        {
            if (Status is "Accepted" or "Done" || DaysToDue is null)
                return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            if (DaysToDue < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (DaysToDue <= 5) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{ClientLabel} - {EntityType} TY{TaxYear} - {Status}" +
        (ComputedDue is { } d ? $" - due {d:MMM d, yyyy} ({DaysToDue}d{(Extended ? ", extended" : string.Empty)})" : string.Empty) +
        (FeeOwed > 0 ? $" - fee owed {FeeOwed:C}" : string.Empty);
}
