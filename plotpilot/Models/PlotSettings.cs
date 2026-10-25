namespace PlotPilot.Models;

public sealed class PlotSettings
{
    public string CemeteryName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    /// <summary>Section names - every cemetery's scheme is its own; blocks/lots/spaces are free text per space.</summary>
    public List<string> Sections { get; set; } = new() { "Section A", "Section B" };
    public List<string> SourceKinds { get; set; } = new()
    {
        "Notebook 1", "Notebook 2", "Deed book", "Plat map annotation", "Staff entry",
    };
}
