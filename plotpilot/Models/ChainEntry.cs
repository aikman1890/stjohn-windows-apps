namespace PlotPilot.Models;

/// <summary>One dated link in a space's ownership chain (sale, transfer, inheritance, note).</summary>
public sealed class ChainEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string SpaceId { get; set; } = string.Empty;
    public string SpaceLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Kind { get; set; } = "Sale"; // Sale / Transfer / Inheritance / Reservation / Note
    public string FromName { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string DeedRef { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal CareContribution { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {Kind}: {FromName} -> {ToName}" +
        (Amount > 0 ? $" - {Amount:C}" : string.Empty) +
        (CareContribution > 0 ? $" - care {CareContribution:C}" : string.Empty) +
        (string.IsNullOrWhiteSpace(DeedRef) ? string.Empty : $" - deed {DeedRef}") +
        (string.IsNullOrWhiteSpace(Source) ? string.Empty : $" - src: {Source}");
}
