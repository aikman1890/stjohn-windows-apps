namespace PlatePilot.Models;

public sealed class QuoteLine
{
    public string RecipeName { get; set; } = string.Empty;
    public int Guests { get; set; }
    public decimal PricePerGuest { get; set; }
    public decimal Amount => Math.Round(Guests * PricePerGuest, 2);
}

public sealed class CateringQuote
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string QuoteNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTimeOffset EventDate { get; set; } = DateTimeOffset.Now.AddDays(14);
    public List<QuoteLine> Lines { get; set; } = new();
    public decimal OtherCharges { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal Total => Lines.Sum(l => l.Amount) + OtherCharges;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{QuoteNumber} - {ClientName} - {EventDate:MMM d, yyyy} - {Total:C} - {Status}";
}
