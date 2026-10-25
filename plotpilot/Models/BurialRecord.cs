namespace PlotPilot.Models;

public sealed class BurialRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string SpaceId { get; set; } = string.Empty;
    public string SpaceLabel { get; set; } = string.Empty;
    public string DecedentName { get; set; } = string.Empty;
    public DateTimeOffset? BirthDate { get; set; }
    public DateTimeOffset? DeathDate { get; set; }
    public DateTimeOffset IntermentDate { get; set; } = DateTimeOffset.Now;
    public string FuneralHome { get; set; } = string.Empty;
    public string MarkerNotes { get; set; } = string.Empty;
    public bool Veteran { get; set; }
    public string Source { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{DecedentName}{(Veteran ? " (veteran)" : string.Empty)} - {SpaceLabel} - interred {IntermentDate:MMM d, yyyy}" +
        (string.IsNullOrWhiteSpace(FuneralHome) ? string.Empty : $" - {FuneralHome}") +
        (string.IsNullOrWhiteSpace(Source) ? string.Empty : $" - src: {Source}");
}
