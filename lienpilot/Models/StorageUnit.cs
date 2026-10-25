using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace LienPilot.Models;

public sealed class StorageUnit
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UnitNumber { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string TenantPhone { get; set; } = string.Empty;
    public string TenantEmail { get; set; } = string.Empty;
    public string TenantAddress { get; set; } = string.Empty;
    public decimal MonthlyRate { get; set; }
    public DateTimeOffset? PaidThrough { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Current";
    public DateTimeOffset? DefaultDate { get; set; }
    public DateTimeOffset? SaleDate { get; set; }
    public bool BankruptcyFlag { get; set; }
    public bool MilitaryFlag { get; set; }
    public bool DisputeFlag { get; set; }
    public string Notes { get; set; } = string.Empty;

    public bool IsDelinquent => Balance > 0 && Status is not ("Released" or "Sold" or "Vacated");
    public bool HasHold => BankruptcyFlag || MilitaryFlag || DisputeFlag;

    public string DisplayLine =>
        $"Unit {UnitNumber} - {TenantName} - {Balance:C} - {Status}" +
        (DefaultDate is { } d ? $" - default {d:MMM d}" : string.Empty);

    public Brush BalanceBrush => Status switch
    {
        "Released" or "Sold" or "Vacated" => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)),
        _ => Balance > 0
            ? new SolidColorBrush(Color.FromArgb(255, 200, 60, 60))
            : new SolidColorBrush(Color.FromArgb(255, 60, 160, 80)),
    };
}
