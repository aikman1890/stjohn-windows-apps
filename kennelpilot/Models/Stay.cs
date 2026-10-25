using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace KennelPilot.Models;

public sealed class Stay
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PetId { get; set; } = string.Empty;
    public string PetLabel { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerLabel { get; set; } = string.Empty;
    public string RunLabel { get; set; } = string.Empty;
    public DateTimeOffset CheckIn { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset ExpectedOut { get; set; } = DateTimeOffset.Now.AddDays(3);
    public DateTimeOffset? ActualOut { get; set; }
    public string Status { get; set; } = "Reserved";
    public string VaccineSnapshot { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public bool IsInHouse => Status is "Checked in";
    public bool IsActive => Status is "Reserved" or "Checked in";
    public int Nights => Math.Max(1, (int)Math.Ceiling(((ActualOut ?? ExpectedOut) - CheckIn).TotalDays));

    public string DisplayLine =>
        $"{PetLabel} - {RunLabel} - {Status} - in {CheckIn:MMM d} to {(ActualOut ?? ExpectedOut):MMM d}";

    public Brush StatusBrush => Status switch
    {
        "Checked out" => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)),
        "Cancelled" => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)),
        "No-show" => new SolidColorBrush(Color.FromArgb(255, 200, 60, 60)),
        _ => CheckIn.Date <= DateTimeOffset.Now.Date && IsInHouse && ExpectedOut.Date < DateTimeOffset.Now.Date
            ? new SolidColorBrush(Color.FromArgb(255, 200, 60, 60))
            : new SolidColorBrush(Color.FromArgb(255, 60, 160, 80)),
    };
}
