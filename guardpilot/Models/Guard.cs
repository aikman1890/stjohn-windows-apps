using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace GuardPilot.Models;

public sealed class Guard
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    /// <summary>Licence data as the firm records it - GuardPilot warns on expiry; it verifies nothing with any state.</summary>
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTimeOffset? LicenseExpiry { get; set; }
    public string Certifications { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active / Inactive
    public string Notes { get; set; } = string.Empty;

    public int? LicenseDays => LicenseExpiry is { } e ? (e.Date - DateTimeOffset.Now.Date).Days : null;

    public Brush LicenseBrush
    {
        get
        {
            if (LicenseDays is null) return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            if (LicenseDays < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (LicenseDays <= 30) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{Name} - {Status}" +
        (LicenseExpiry is { } e ? $" - licence to {e:MMM d, yyyy} ({LicenseDays}d)" : " - no licence date recorded");
}
