namespace WellPilot.Models;

public sealed class ProductionMonth
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string WellId { get; set; } = string.Empty;
    public string WellLabel { get; set; } = string.Empty;
    /// <summary>Production month, stored as yyyy-MM.</summary>
    public string Month { get; set; } = DateTimeOffset.Now.AddMonths(-1).ToString("yyyy-MM");
    public decimal OilBbl { get; set; }
    public decimal GasMcf { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal SeveranceTax { get; set; }
    public decimal Deductions { get; set; }
    public string PurchaserRef { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public decimal NetRevenue => GrossRevenue - SeveranceTax - Deductions;

    public string DisplayLine =>
        $"{Month} - {WellLabel} - {OilBbl:0.0} bbl / {GasMcf:0.0} MCF - gross {GrossRevenue:C} - net {NetRevenue:C}";
}
