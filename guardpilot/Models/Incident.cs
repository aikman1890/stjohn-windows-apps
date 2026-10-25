using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace GuardPilot.Models;

public sealed class Incident
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset When { get; set; } = DateTimeOffset.Now;
    public string SiteLabel { get; set; } = string.Empty;
    public string PostLabel { get; set; } = string.Empty;
    public string GuardLabel { get; set; } = string.Empty;
    public string Type { get; set; } = "Trespass";
    public string Narrative { get; set; } = string.Empty;
    public string FollowUp { get; set; } = string.Empty;
    public string FollowUpOwner { get; set; } = string.Empty;
    public DateTimeOffset? FollowUpDue { get; set; }
    public bool FollowUpDone { get; set; }

    public bool FollowUpOverdue => !FollowUpDone && FollowUpDue is { } d && d.Date < DateTimeOffset.Now.Date &&
        !string.IsNullOrWhiteSpace(FollowUp);

    public Brush FlagBrush => FollowUpOverdue
        ? new SolidColorBrush(Color.FromArgb(255, 200, 60, 60))
        : new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));

    public string DisplayLine =>
        $"{When:MMM d, yyyy h:mm tt} - {Type} - {PostLabel} - {GuardLabel}" +
        (FollowUpOverdue ? " - FOLLOW-UP OVERDUE" : string.Empty);
}
