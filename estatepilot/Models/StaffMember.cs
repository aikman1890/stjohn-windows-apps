namespace EstatePilot.Models;

public sealed class StaffMember
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PropertyId { get; set; } = string.Empty;
    public string PropertyLabel { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string SchedulePattern { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public DateTimeOffset? StartDate { get; set; }
    /// <summary>Document clocks the manager tracks (certifications, background-check renewals) - user-entered, user-confirmed. Records only; no payroll or HR functions.</summary>
    public string DocNote { get; set; } = string.Empty;
    public DateTimeOffset? DocExpiry { get; set; }
    public string Notes { get; set; } = string.Empty;

    public int? DocDays => DocExpiry is { } d ? (d.Date - DateTimeOffset.Now.Date).Days : null;

    public string DisplayLine =>
        $"{Name} - {Role} - {PropertyLabel} - {SchedulePattern}" +
        (DocDays is { } dd ? $" - document clock: {(dd < 0 ? $"EXPIRED {-dd} days ago" : $"expires in {dd} days")}" : string.Empty);
}
