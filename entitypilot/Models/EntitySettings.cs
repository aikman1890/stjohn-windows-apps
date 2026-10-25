namespace EntityPilot.Models;

public sealed class EntitySettings
{
    public string OwnerName { get; set; } = string.Empty;
    public int OrangeDays { get; set; } = 30;
    /// <summary>
    /// Starter templates: "ST|Obligation|Typical timing (starting point - CONFIRM on the state portal)|Reviewed yyyy-MM-dd".
    /// Starting points only - never deadline authority.
    /// </summary>
    public List<string> StateTemplates { get; set; } = new()
    {
        "TX|Franchise tax report|Due May 15 yearly for most entities (starting point - CONFIRM)|2026-10-25",
        "TX|Registered agent|Agent must be maintained continuously; renewal follows the agent's own contract (CONFIRM)|2026-10-25",
        "WY|Annual report|Due the 1st of the formation anniversary month yearly (starting point - CONFIRM)|2026-10-25",
        "DE|Franchise tax (LLC/LP)|Due June 1 yearly, flat tax (starting point - CONFIRM)|2026-10-25",
        "DE|Franchise tax (corp)|Due March 1 yearly (starting point - CONFIRM)|2026-10-25",
        "FL|Annual report|Due January 1 - May 1 yearly (starting point - CONFIRM)|2026-10-25",
        "CA|Statement of Information|Due within 90 days of formation, then every 1-2 years by entity type (starting point - CONFIRM)|2026-10-25",
        "NV|Annual list + business license|Due the last day of the anniversary month yearly (starting point - CONFIRM)|2026-10-25",
    };
}
