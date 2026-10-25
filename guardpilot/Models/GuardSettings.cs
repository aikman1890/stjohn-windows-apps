namespace GuardPilot.Models;

public sealed class GuardSettings
{
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal OvertimeWeeklyThreshold { get; set; } = 40m;
    public List<string> IncidentTypes { get; set; } = new()
    {
        "Trespass", "Alarm response", "Property damage", "Injury", "Police called",
        "Suspicious activity", "Lockout / access", "Other",
    };
}
