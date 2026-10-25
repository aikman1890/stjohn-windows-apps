using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace WheelPilot.Models;

public sealed class Student
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string ParentPhone { get; set; } = string.Empty;
    /// <summary>Permit data as the school records it - records, not verification.</summary>
    public string PermitNumber { get; set; } = string.Empty;
    public DateTimeOffset? PermitExpiry { get; set; }
    public string Course { get; set; } = string.Empty;
    public string Status { get; set; } = "Enrolled"; // Inquiry / Enrolled / In training / Test-ready / Passed / Completed / Inactive
    public string Source { get; set; } = string.Empty;
    public DateTimeOffset? LastContacted { get; set; }
    public DateTimeOffset? TestDate { get; set; }
    public string TestNote { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public bool InquiryStale => Status == "Inquiry" && LastContacted is { } l && (DateTimeOffset.Now - l).TotalDays > 5;
    public int? PermitDays => PermitExpiry is { } e ? (e.Date - DateTimeOffset.Now.Date).Days : null;

    public Brush FlagBrush
    {
        get
        {
            if (InquiryStale) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (PermitDays is { } d && d < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (PermitDays is { } d2 && d2 <= 30) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{Name} - {Status}{(string.IsNullOrWhiteSpace(Course) ? string.Empty : $" - {Course}")}" +
        (InquiryStale ? " - INQUIRY STALE" : string.Empty);
}
