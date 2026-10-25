namespace WheelPilot.Models;

public sealed class AttendanceEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StudentId { get; set; } = string.Empty;
    public string StudentLabel { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string SessionLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Course} / {SessionLabel} - {StudentLabel}";
}
