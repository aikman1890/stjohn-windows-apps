using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace YachtPilot.Models;

public sealed class Vendor
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Trade { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public DateTimeOffset? CoiExpiry { get; set; }
    public string LastJobNote { get; set; } = string.Empty;
    public string NextBookedNote { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public Brush FlagBrush
    {
        get
        {
            if (CoiExpiry is { } e)
            {
                var days = (e.Date - DateTimeOffset.Now.Date).Days;
                if (days < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
                if (days <= 30) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            }
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{Name} - {Trade} - {Contact}" + (CoiExpiry is { } e ? $" - COI expires {e:MMM d, yyyy}" : string.Empty);
}
