namespace WheelPilot.Models;

public sealed class MilestoneMark
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StudentId { get; set; } = string.Empty;
    public string Skill { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string SignedBy { get; set; } = string.Empty;
}
