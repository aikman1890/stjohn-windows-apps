namespace YachtPilot.Models;

public sealed class YachtDoc
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string VesselId { get; set; } = string.Empty;
    public string VesselLabel { get; set; } = string.Empty;
    public string Kind { get; set; } = "Certificate"; // Certificate / Survey / Manual / Insurance / Registration / Other
    public string Title { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;

    public string DisplayLine => $"{VesselLabel} - {Kind} - {Title} - {LocalPath}";
}
