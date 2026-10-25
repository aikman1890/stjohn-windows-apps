using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AgreementPilot.Models;

public sealed class Equipment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerLabel { get; set; } = string.Empty;
    public string Type { get; set; } = "Furnace";
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;
    public DateTimeOffset? InstallDate { get; set; }
    public string LocationNote { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    /// <summary>Whole years old, date-only math (no time-zone drift on anniversaries).</summary>
    public int? AgeYears
    {
        get
        {
            if (InstallDate is not { } d) return null;
            var today = DateTimeOffset.Now.Date;
            var age = today.Year - d.Date.Year;
            if (d.Date.AddYears(age) > today) age--;
            return age;
        }
    }

    public string FullLabel =>
        $"{Type} {Brand} {Model}".Trim() + (AgeYears is { } a ? $" - {a} yr old" : " - age unknown") +
        $" - {CustomerLabel}";

    public Brush AgeBrush => AgeYears switch
    {
        null => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)),
        { } a when a >= 20 => new SolidColorBrush(Color.FromArgb(255, 200, 60, 60)),
        { } a when a >= 15 => new SolidColorBrush(Color.FromArgb(255, 220, 140, 30)),
        _ => new SolidColorBrush(Color.FromArgb(255, 60, 160, 80)),
    };
}
