namespace ClaimPilot.Models;

/// <summary>A plain-English card mapping described conduct to a possibly-relevant law.</summary>
public sealed class ViolationCard
{
    public ViolationCard(string lawName, string explanation)
    {
        LawName = lawName;
        Explanation = explanation;
    }

    public string LawName { get; }
    public string Explanation { get; }

    /// <summary>Fixed safety copy shown on every card.</summary>
    public string AppliesNote => Services.LegalText.MayApplyNote;
}
