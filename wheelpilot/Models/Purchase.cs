namespace WheelPilot.Models;

public sealed class Purchase
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StudentId { get; set; } = string.Empty;
    public string StudentLabel { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal Price { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Method { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public decimal Owed => Price - PaidAmount;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {PackageName} - {Hours:0.0}h - {Price:C} (paid {PaidAmount:C}) - {StudentLabel}";
}
