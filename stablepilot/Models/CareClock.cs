using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace StablePilot.Models;

public sealed class CareClock
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string HorseId { get; set; } = string.Empty;
    public string HorseLabel { get; set; } = string.Empty;
    /// <summary>Coggins / Farrier / Vaccination / Deworming / Dental / CVI / Other.</summary>
    public string Kind { get; set; } = "Farrier";
    public string Detail { get; set; } = string.Empty;
    public DateTimeOffset? LastDoneDate { get; set; }
    public DateTimeOffset NextDueDate { get; set; } = DateTimeOffset.Now.AddMonths(2);
    /// <summary>Cycle length used when an item is marked done (days).</summary>
    public int CycleDays { get; set; } = 45;

    public int DaysUntil => (NextDueDate.Date - DateTimeOffset.Now.Date).Days;

    /// <summary>Coggins gets the wider 30-day orange band (statutory paperwork); care cycles use 14.</summary>
    public int OrangeBand => Kind == "Coggins" ? 30 : 14;

    public Brush FlagBrush
    {
        get
        {
            if (DaysUntil < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (DaysUntil <= OrangeBand) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{HorseLabel} - {Kind}{(string.IsNullOrWhiteSpace(Detail) ? string.Empty : $" ({Detail})")} - due {NextDueDate:MMM d, yyyy} " +
        (DaysUntil < 0 ? $"- OVERDUE by {-DaysUntil} days{(Kind == "Coggins" ? " - NOT TRAVEL-LEGAL" : string.Empty)}"
            : $"- in {DaysUntil} days") +
        (LastDoneDate is { } l ? $" - last {l:MMM d, yyyy}" : string.Empty);
}
