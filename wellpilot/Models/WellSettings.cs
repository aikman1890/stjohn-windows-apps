namespace WellPilot.Models;

public sealed class WellSettings
{
    public string OperatorName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public List<string> ExpenseCategories { get; set; } = new()
    {
        "Labor", "Chemicals", "Repairs", "Hauling", "Utilities", "Contract services", "Other",
    };
}
