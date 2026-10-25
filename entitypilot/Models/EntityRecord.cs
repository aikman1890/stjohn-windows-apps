using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace EntityPilot.Models;

public sealed class EntityRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string LegalName { get; set; } = string.Empty;
    public string Type { get; set; } = "LLC"; // LLC / S-corp / C-corp / LP / PLLC / Other
    public string State { get; set; } = string.Empty;
    public DateTimeOffset FormationDate { get; set; } = DateTimeOffset.Now;
    public string FileNumber { get; set; } = string.Empty;
    /// <summary>Stored locally only. Masked by default in the UI; never exported unless the owner opts in.</summary>
    public string Ein { get; set; } = string.Empty;
    public string FiscalYearEnd { get; set; } = "12-31";
    /// <summary>The owner's own record of standing - never a live state claim.</summary>
    public string Status { get; set; } = "Unknown"; // Active / Inactive / Dissolved / Unknown
    public DateTimeOffset? StatusConfirmedDate { get; set; }
    public string RegisteredAgentName { get; set; } = string.Empty;
    public string AgentAddress { get; set; } = string.Empty;
    public string AgentFeeNote { get; set; } = string.Empty;
    public DateTimeOffset? AgentRenewalDate { get; set; }
    /// <summary>Pointer only (site + username note). Passwords are NEVER stored here.</summary>
    public string PortalReference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string EinMasked =>
        string.IsNullOrWhiteSpace(Ein) ? "(not recorded)"
        : Ein.Length <= 4 ? "••-•••••••"
        : "••-••••" + Ein[^3..];

    public int? AgentDays => AgentRenewalDate is { } d ? (d.Date - DateTimeOffset.Now.Date).Days : null;

    public Brush FlagBrush
    {
        get
        {
            if (Status is "Dissolved" or "Inactive") return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (AgentDays is { } d && d < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
            if (AgentDays is { } d2 && d2 <= 30) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            if (Status == "Unknown") return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
            return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        }
    }

    public string DisplayLine =>
        $"{LegalName} - {Type} - {State} - status: {Status}" +
        (StatusConfirmedDate is { } c ? $" (confirmed {c:MMM d, yyyy})" : " (never confirmed)");
}
