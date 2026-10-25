namespace WellPilot.Models;

public sealed class WellExpense
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string WellId { get; set; } = string.Empty;
    public string WellLabel { get; set; } = string.Empty;
    public string Month { get; set; } = DateTimeOffset.Now.AddMonths(-1).ToString("yyyy-MM");
    public string Category { get; set; } = "Labor"; // Labor / Chemicals / Repairs / Hauling / Utilities / Contract services / Other
    public decimal Amount { get; set; }
    public string Vendor { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public string DisplayLine => $"{Month} - {WellLabel} - {Category} - {Amount:C} - {Vendor}";
}
