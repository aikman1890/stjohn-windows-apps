namespace StablePilot.Models;

public sealed class HorseDoc
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string HorseId { get; set; } = string.Empty;
    public string HorseLabel { get; set; } = string.Empty;
    /// <summary>Coggins certificate / Health papers / Registration / Insurance / Other.</summary>
    public string Kind { get; set; } = "Coggins certificate";
    public string Title { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;

    public string DisplayLine => $"{HorseLabel} - {Kind} - {Title} - {LocalPath}";
}
