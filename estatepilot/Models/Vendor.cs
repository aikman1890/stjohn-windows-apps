using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace EstatePilot.Models;

public sealed class Vendor
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Trade { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string PropertiesNote { get; set; } = string.Empty;
    public string LastJobNote { get; set; } = string.Empty;
    public string RateNote { get; set; } = string.Empty;
    /// <summary>Certificate-of-insurance expiry as a first-class clock: who is currently insured to work in the house.</summary>
    public DateTimeOffset? CoiExpiry { get; set; }
    public string Notes { get; set; } = string.Empty;

    public bool InsuredNow => CoiExpiry is { } e && e.Date >= DateTimeOffset.Now.Date;

    public Brush FlagBrush
    {
        get
        {
            if (CoiExpiry is { } e)
            {
                var days = (e.Date - DateTimeOffset.Now.Date).Days;
                if (days < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
                if (days <= 30) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
                return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
            }
            return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
        }
    }

    public string DisplayLine =>
        $"{Name} - {Trade} - {Contact} - COI: {(CoiExpiry is { } e ? $"expires {e:MMM d, yyyy}{(InsuredNow ? string.Empty : " - NOT CURRENT")}" : "none on file")}";
}
