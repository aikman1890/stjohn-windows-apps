namespace MovePilot.Models;

public sealed class StorageLot
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Customer { get; set; } = string.Empty;
    public string LotId { get; set; } = string.Empty;
    public DateTimeOffset InDate { get; set; } = DateTimeOffset.Now;
    public decimal MonthlyRate { get; set; }
    public DateTimeOffset? OutDate { get; set; }
    public decimal PaidToDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    /// <summary>Storage is billed per started 30-day month.</summary>
    public int MonthsBilled
    {
        get
        {
            var end = OutDate ?? DateTimeOffset.Now;
            var days = Math.Max(1, (end.Date - InDate.Date).Days + 1);
            return (int)Math.Ceiling(days / 30.0);
        }
    }

    public decimal Accrued => MonthsBilled * MonthlyRate;
    public decimal Balance => Accrued - PaidToDate;

    public string DisplayLine =>
        $"Lot {LotId} - {Customer} - in {InDate:MMM d, yyyy}" +
        (OutDate is { } o ? $" - out {o:MMM d, yyyy}" : " - in storage") +
        $" - {MonthlyRate:C}/mo x {MonthsBilled} = {Accrued:C} - paid {PaidToDate:C} - balance {Balance:C}";
}
