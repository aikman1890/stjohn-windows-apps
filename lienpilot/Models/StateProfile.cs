namespace LienPilot.Models;

/// <summary>
/// The operator's own state lien profile, entered from their statute and
/// confirmed by them. LienPilot ships NO law: one clearly-labeled SAMPLE
/// profile is seeded unconfirmed, and nothing computes as "ready" from an
/// unconfirmed profile without the operator seeing that fact.
/// </summary>
public sealed class StateProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public bool IsSample { get; set; }
    public int NoticeDayOffset { get; set; } = 5;      // days after default the lien notice step comes due
    public int WaitingDaysAfterNotice { get; set; } = 30; // when no publication step applies
    public bool PublicationRequired { get; set; }
    public int SaleDaysAfterFirstPublication { get; set; } = 15;
    public List<string> RequiredSteps { get; set; } = new();
    public bool Confirmed { get; set; }
    public DateTimeOffset ConfirmedDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Name}{(IsSample ? " (SAMPLE)" : "")} - notice +{NoticeDayOffset}d, wait {WaitingDaysAfterNotice}d" +
        (PublicationRequired ? $", publication + sale {SaleDaysAfterFirstPublication}d after first pub" : ", no publication step") +
        (Confirmed ? $" - confirmed {ConfirmedDate:MMM d, yyyy}" : " - NOT confirmed");
}
