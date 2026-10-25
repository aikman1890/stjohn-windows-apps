namespace WellPilot.Models;

public sealed class Well
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string LeaseName { get; set; } = string.Empty;
    public string WellName { get; set; } = string.Empty;
    public string ApiNumber { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string State { get; set; } = "TX";
    public string Operator { get; set; } = string.Empty;
    public string Status { get; set; } = "Producing"; // Producing / Shut-in / Plugged
    public DateTimeOffset? FirstProductionDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{LeaseName} / {WellName} - {County} Co., {State} - {Status}";
}
