namespace GuardPilot.Models;

/// <summary>Daily activity report entry, logged at the office from whatever the guard submitted.</summary>
public sealed class DarEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string PostLabel { get; set; } = string.Empty;
    public string GuardLabel { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {PostLabel} - {GuardLabel}: {Summary}";
}
