using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace WellPilot.Models;

public sealed class ComplianceDate
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTimeOffset DueDate { get; set; } = DateTimeOffset.Now.AddDays(14);
    public string Recurrence { get; set; } = "Monthly"; // One-time / Monthly / Quarterly / Annual
    public string Notes { get; set; } = string.Empty;

    public int DaysToDue => (DueDate.Date - DateTimeOffset.Now.Date).Days;

    public Brush DueBrush
    {
        get
        {
            if (DaysToDue < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (DaysToDue <= 5) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine => $"{Title} - due {DueDate:MMM d, yyyy} ({DaysToDue} days) - {Recurrence}";
}
