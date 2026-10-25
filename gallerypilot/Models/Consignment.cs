using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace GalleryPilot.Models;

public sealed class Consignment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ConsignorName { get; set; } = string.Empty;
    /// <summary>Artist / Collector / Estate.</summary>
    public string ConsignorKind { get; set; } = "Artist";
    public string Contact { get; set; } = string.Empty;
    /// <summary>The consignor's share of a sale, in percent. Splits are computed from this - totals are never typed.</summary>
    public decimal SplitPercent { get; set; } = 50m;
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset TermEnd { get; set; } = DateTimeOffset.Now.AddMonths(6);
    /// <summary>Active / Renewed / Returned / Closed.</summary>
    public string Status { get; set; } = "Active";
    public string TermsNote { get; set; } = string.Empty;

    public int DaysUntilEnd => (TermEnd.Date - DateTimeOffset.Now.Date).Days;
    public bool Lapsed => Status == "Active" && DaysUntilEnd < 0;

    public Brush FlagBrush
    {
        get
        {
            if (Status != "Active") return new SolidColorBrush(Color.FromArgb(255, 140, 140, 140));
            if (DaysUntilEnd < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (DaysUntilEnd <= 30) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{ConsignorName} ({ConsignorKind}) - split {SplitPercent:0}% - term ends {TermEnd:MMM d, yyyy} ({(DaysUntilEnd < 0 ? $"LAPSED {-DaysUntilEnd} days ago - the gallery may no longer have the right to sell these works" : $"in {DaysUntilEnd} days")}) - {Status}";
}
