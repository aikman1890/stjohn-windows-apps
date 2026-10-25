namespace ShopPilot.Models;

public sealed class JobOperation
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string JobId { get; set; } = string.Empty;
    public int Seq { get; set; }
    public string Name { get; set; } = string.Empty;
    public string WorkCenter { get; set; } = string.Empty;
    public decimal PlannedHours { get; set; }
    public decimal ActualHours { get; set; }
    public bool Done { get; set; }
    public string SignOff { get; set; } = string.Empty;
    public DateTimeOffset? SignDate { get; set; }

    public string DisplayLine =>
        $"{Seq}. {Name} @ {WorkCenter} - planned {PlannedHours:0.0}h, actual {ActualHours:0.0}h" +
        (Done ? $" - DONE by {SignOff} {SignDate:MMM d}" : " - open");
}
