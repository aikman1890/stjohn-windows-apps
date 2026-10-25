namespace GalleryPilot.Models;

public sealed class Exhibition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.Now.AddMonths(1);
    public string WorksNote { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Title} - {Venue} - {StartDate:MMM d} to {EndDate:MMM d, yyyy} - works: {WorksNote}";
}
