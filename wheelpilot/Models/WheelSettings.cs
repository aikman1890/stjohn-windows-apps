namespace WheelPilot.Models;

public sealed class WheelSettings
{
    public string SchoolName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    /// <summary>Packages: "Name|Hours|Price".</summary>
    public List<string> Packages { get; set; } = new()
    {
        "6-hour Behind-the-Wheel|6|540",
        "10-hour + Test Car|10|925",
        "Classroom Only|0|250",
        "Single Lesson|1|95",
    };
    /// <summary>Behind-the-wheel milestone skills.</summary>
    public List<string> Skills { get; set; } = new()
    {
        "Residential driving", "Parking (parallel)", "Highway merging", "Night drive",
        "Downtown traffic", "Three-point turn", "Defensive commentary drive", "Mock road test",
    };
    /// <summary>Courses: "Course|Session|Session..." - classroom session lists per course.</summary>
    public List<string> Courses { get; set; } = new()
    {
        "Teen Course (sample)|Class 1 - Signs & signals|Class 2 - Right of way|Class 3 - Defensive driving|Class 4 - Impairment & distraction|Class 5 - Sharing the road|Class 6 - Final review",
    };
}
