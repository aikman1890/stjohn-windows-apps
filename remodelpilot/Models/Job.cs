namespace RemodelPilot.Models;

public sealed class Job
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ClientName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContractType { get; set; } = "Fixed price";
    /// <summary>Contract price from the winning estimate version. Approved COs add on top.</summary>
    public decimal BaseContractPrice { get; set; }
    public string WinningVersionId { get; set; } = string.Empty;
    public string Status { get; set; } = "Bid";
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? CompleteDate { get; set; }
    public DateTimeOffset? WarrantyEnd { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{ClientName} - {Address} - {Status}";
}
