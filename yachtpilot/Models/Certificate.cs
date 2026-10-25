using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace YachtPilot.Models;

public sealed class Certificate
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CrewId { get; set; } = string.Empty;
    public string CrewLabel { get; set; } = string.Empty;
    /// <summary>Free-form: STCW Basic, ENG1 Medical, Master Licence, Passport, Visa, etc.</summary>
    public string Type { get; set; } = string.Empty;
    public DateTimeOffset? IssuedDate { get; set; }
    public DateTimeOffset ExpiryDate { get; set; } = DateTimeOffset.Now;
    public string Note { get; set; } = string.Empty;

    public int DaysUntil => (ExpiryDate.Date - DateTimeOffset.Now.Date).Days;
    public bool Expired => DaysUntil < 0;

    public Brush FlagBrush
    {
        get
        {
            if (Expired) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (DaysUntil <= 30) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{CrewLabel} - {Type} - expires {ExpiryDate:MMM d, yyyy} ({(Expired ? $"EXPIRED {-DaysUntil} days ago" : $"in {DaysUntil} days")})";
}
