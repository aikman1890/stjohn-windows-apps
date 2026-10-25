namespace FarewellPilot.Models;

public sealed class ContractLine
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CaseId { get; set; } = string.Empty;
    public string Category { get; set; } = "Professional services";
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public string DisplayLine => $"{Category}: {Description} - {Amount:C}";
}
