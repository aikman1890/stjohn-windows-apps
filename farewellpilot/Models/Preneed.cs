namespace FarewellPilot.Models;

public sealed class PreneedContract
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Number { get; set; } = string.Empty;
    public string Purchaser { get; set; } = string.Empty;
    public string Beneficiary { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public decimal TotalAmount { get; set; }
    public string FundingType { get; set; } = "Trust"; // Trust / Insurance
    public string FundingRef { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active / Fulfilled / Transferred / Cancelled
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Number} - {Beneficiary} (purchaser {Purchaser}) - {TotalAmount:C} - {FundingType} - {Status}";
}

public sealed class DepositEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PreneedId { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public decimal Amount { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Amount:C} - {Destination}" +
        (string.IsNullOrWhiteSpace(Note) ? string.Empty : $" - {Note}");
}
