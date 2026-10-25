namespace GuardPilot.Models;

public sealed class Post
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string SiteId { get; set; } = string.Empty;
    public string SiteLabel { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{SiteLabel} / {Name}";
}
