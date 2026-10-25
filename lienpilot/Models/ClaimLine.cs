namespace LienPilot.Models;

public sealed class ClaimLine
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UnitId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
}
