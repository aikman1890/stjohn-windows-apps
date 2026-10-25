using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace EstatePilot.Models;

public sealed class ServiceClock
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PropertyId { get; set; } = string.Empty;
    public string PropertyLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset? LastDoneDate { get; set; }
    public DateTimeOffset NextDueDate { get; set; } = DateTimeOffset.Now.AddMonths(6);
    public int IntervalMonths { get; set; } = 12;
    public string Note { get; set; } = string.Empty;

    public int DaysUntil => (NextDueDate.Date - DateTimeOffset.Now.Date).Days;

    public Brush FlagBrush
    {
        get
        {
            if (DaysUntil < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (DaysUntil <= 30) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{PropertyLabel} - {Title} - due {NextDueDate:MMM d, yyyy} ({(DaysUntil < 0 ? $"OVERDUE by {-DaysUntil} days" : $"in {DaysUntil} days")})" +
        (LastDoneDate is { } l ? $" - last done {l:MMM d, yyyy}" : " - never logged");
}
