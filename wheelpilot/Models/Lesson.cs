namespace WheelPilot.Models;

public sealed class Lesson
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StudentId { get; set; } = string.Empty;
    public string StudentLabel { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public string InstructorLabel { get; set; } = string.Empty;
    public string CarId { get; set; } = string.Empty;
    public string CarLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public decimal StartHour { get; set; } = 15m;
    public decimal DurationHours { get; set; } = 1.5m;
    public string Status { get; set; } = "Scheduled"; // Scheduled / Completed / Cancelled / No-show
    public string PickupLocation { get; set; } = string.Empty;
    public string SkillsCovered { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string NextRecommendation { get; set; } = string.Empty;

    public decimal EndHour => StartHour + DurationHours;

    public string DisplayLine =>
        $"{Date:ddd, MMM d} {StartHour:0.0}-{EndHour:0.0}h - {StudentLabel} with {InstructorLabel} in {CarLabel} - {Status}";
}
