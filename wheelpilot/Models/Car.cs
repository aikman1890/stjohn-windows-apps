using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace WheelPilot.Models;

public sealed class Car
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string PlateText { get; set; } = string.Empty;
    public DateTimeOffset? InspectionExpiry { get; set; }
    public DateTimeOffset? RegistrationExpiry { get; set; }
    public DateTimeOffset? InsuranceExpiry { get; set; }
    public bool OutOfService { get; set; }
    public string OutOfServiceNote { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset? NearestExpiry
    {
        get
        {
            var dates = new[] { InspectionExpiry, RegistrationExpiry, InsuranceExpiry }
                .Where(d => d is not null).Select(d => d!.Value).ToList();
            return dates.Count > 0 ? dates.Min() : null;
        }
    }

    public Brush FlagBrush
    {
        get
        {
            if (OutOfService) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (NearestExpiry is { } e)
            {
                var days = (e.Date - DateTimeOffset.Now.Date).Days;
                if (days < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
                if (days <= 30) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            }
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{Name} ({PlateText})" + (OutOfService ? $" - OUT OF SERVICE: {OutOfServiceNote}" : string.Empty);
}
