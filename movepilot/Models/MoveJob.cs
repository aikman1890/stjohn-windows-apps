namespace MovePilot.Models;

public sealed class MoveJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Customer { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTimeOffset MoveDate { get; set; } = DateTimeOffset.Now.AddDays(7);
    public string Status { get; set; } = "Lead"; // Lead / Surveyed / Booked / Packed / Moved / Delivered / Closed / Claim open
    public string AccessNotes { get; set; } = string.Empty;
    public string ValuationChoice { get; set; } = string.Empty;
    public int CrewSize { get; set; } = 3;
    public int Trucks { get; set; } = 1;
    public decimal ContractAmount { get; set; }
    public decimal Deposit { get; set; }
    public decimal Collected { get; set; }
    public string Notes { get; set; } = string.Empty;

    public decimal Balance => ContractAmount - Deposit - Collected;

    public string DisplayLine =>
        $"{Customer} - {MoveDate:MMM d, yyyy} - {Status} - {Origin} -> {Destination}" +
        (ContractAmount > 0 ? $" - contract {ContractAmount:C}, balance {Balance:C}" : string.Empty);
}
