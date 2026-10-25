namespace RemodelPilot.Models;

public sealed class PunchItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string JobId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Done { get; set; }
    public DateTimeOffset? DoneDate { get; set; }

    public string DisplayLine => $"{(Done ? "[done] " : "")}{Description}" +
        (DoneDate is { } d ? $" - {d:MMM d, yyyy}" : string.Empty);
}
