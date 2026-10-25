using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PlotPilot.Models;

public sealed class PlotSpace
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Section { get; set; } = string.Empty;
    public string Block { get; set; } = string.Empty;
    public string Lot { get; set; } = string.Empty;
    public string Space { get; set; } = string.Empty;
    /// <summary>Available / Sold / Reserved / Occupied / Unknown (old records have gaps - Unknown is honest).</summary>
    public string Status { get; set; } = "Available";
    public string OwnerName { get; set; } = string.Empty;
    public string DeedRef { get; set; } = string.Empty;
    public DateTimeOffset? PurchaseDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal CareContribution { get; set; }
    /// <summary>Provenance: which notebook/page, deed book, or plat annotation this entry came from.</summary>
    public string Source { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string Address
    {
        get
        {
            var parts = new[] { Section, Block, Lot, Space }.Where(p => !string.IsNullOrWhiteSpace(p));
            return string.Join(" / ", parts);
        }
    }

    public Brush StatusBrush => Status switch
    {
        "Available" => new SolidColorBrush(Color.FromArgb(255, 60, 160, 80)),
        "Sold" => new SolidColorBrush(Color.FromArgb(255, 90, 130, 200)),
        "Reserved" => new SolidColorBrush(Color.FromArgb(255, 220, 140, 30)),
        "Occupied" => new SolidColorBrush(Color.FromArgb(255, 120, 120, 120)),
        _ => new SolidColorBrush(Color.FromArgb(255, 170, 90, 160)),
    };

    public string DisplayLine =>
        $"{Address} - {Status}" +
        (string.IsNullOrWhiteSpace(OwnerName) ? string.Empty : $" - owner: {OwnerName}") +
        (string.IsNullOrWhiteSpace(Source) ? string.Empty : $" - src: {Source}");
}
