namespace GalleryPilot.Models;

public sealed class Sale
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ArtworkId { get; set; } = string.Empty;
    public string ArtworkLabel { get; set; } = string.Empty;
    public string ArtistLabel { get; set; } = string.Empty;
    public string CollectorLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public decimal SalePrice { get; set; }
    public string Currency { get; set; } = "USD";
    /// <summary>Snapshot of the split at sale time - later renegotiations never rewrite a closed sale.</summary>
    public decimal SplitPercent { get; set; }
    public DateTimeOffset DueDate { get; set; } = DateTimeOffset.Now.AddDays(30);
    public DateTimeOffset? PaidDate { get; set; }
    public string Note { get; set; } = string.Empty;

    /// <summary>Computed from the stored split - never a typed total.</summary>
    public decimal ArtistOwed => Math.Round(SalePrice * SplitPercent / 100m, 2, MidpointRounding.AwayFromZero);
    public decimal GalleryKeeps => SalePrice - ArtistOwed;
    public bool IsPaid => PaidDate is not null;
    public int DaysOverdue => IsPaid ? 0 : (DateTimeOffset.Now.Date - DueDate.Date).Days;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {ArtworkLabel} - sold {SalePrice:0.00} {Currency} to {CollectorLabel} - artist owed {ArtistOwed:0.00} " +
        (IsPaid ? $"(paid {PaidDate:MMM d})" : DaysOverdue > 0 ? $"(UNPAID, {DaysOverdue} days past due {DueDate:MMM d})" : $"(due {DueDate:MMM d})");
}
