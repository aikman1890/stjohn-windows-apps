using System.Collections.ObjectModel;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClaimPilot.ViewModels;

/// <summary>
/// Violation Spotter: maps questionnaire answers to plain-English law cards.
/// Every card carries the fixed "MAY apply / general information" safety copy.
/// </summary>
public partial class ViolationSpotterViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _callsBefore8amOrAfter9pm;

    [ObservableProperty]
    private bool _threatsOfArrestViolenceHarm;

    [ObservableProperty]
    private bool _falseOrInflatedAmounts;

    [ObservableProperty]
    private bool _continuedCollectionAfterDispute;

    [ObservableProperty]
    private bool _contactedEmployerFamilyAboutDebt;

    [ObservableProperty]
    private bool _neverSentWrittenValidation;

    [ObservableProperty]
    private bool _addedUnauthorizedFees;

    public ObservableCollection<ViolationCard> Results { get; } = new();

    [ObservableProperty]
    private bool _hasResults;

    public string IntroText =>
        "Check each box that describes what the debt collector did. ClaimPilot will show plain-English cards " +
        "about the federal laws that MAY apply. This is general information, not legal advice.";

    public string DisclaimerText => LegalText.Disclaimer;

    [RelayCommand]
    private void Analyze()
    {
        Results.Clear();

        if (CallsBefore8amOrAfter9pm || ContactedEmployerFamilyAboutDebt)
        {
            Results.Add(new ViolationCard(
                "FDCPA — 15 U.S.C. 1692c (time, place, and third-party communications)",
                "The Fair Debt Collection Practices Act limits when and with whom a collector may communicate. Calls before 8:00 a.m. or after 9:00 p.m. " +
                "your time are presumed to be at an inconvenient time. Collectors also generally may not discuss your debt with third parties such as your " +
                "employer, family members, or friends — with narrow exceptions (for example, your spouse, parent, or attorney). If the conduct you checked " +
                "happened, document the dates, times, and who was contacted."));
        }

        if (ThreatsOfArrestViolenceHarm)
        {
            Results.Add(new ViolationCard(
                "FDCPA — 15 U.S.C. 1692d (harassment, threats, and abuse)",
                "Collectors may not harass, oppress, or abuse you. That includes threats of violence or harm, threats to have you arrested or jailed over a " +
                "civil debt, and threats to take actions the collector cannot legally take or does not actually intend to take. Save voicemails, texts, and " +
                "letters, and write down exactly what was said, when, and by whom."));
        }

        if (FalseOrInflatedAmounts)
        {
            Results.Add(new ViolationCard(
                "FDCPA — 15 U.S.C. 1692e (false or misleading representations)",
                "Collectors may not use false, deceptive, or misleading representations — including misrepresenting the character, amount, or legal status " +
                "of a debt. Inflating what you owe, claiming you owe a debt that is not yours, or misrepresenting who the collector is can fall here. " +
                "Keep every statement and compare the numbers against your own records."));
        }

        if (AddedUnauthorizedFees)
        {
            Results.Add(new ViolationCard(
                "FDCPA — 15 U.S.C. 1692f (unfair practices)",
                "Collectors may not use unfair or unconscionable means to collect a debt. That includes trying to collect fees, interest, or charges that " +
                "were not authorized by your original agreement and are not otherwise permitted by law. Ask the collector to identify, in writing, the " +
                "contractual or legal basis for every added charge."));
        }

        if (NeverSentWrittenValidation || ContinuedCollectionAfterDispute)
        {
            Results.Add(new ViolationCard(
                "FDCPA — 15 U.S.C. 1692g (validation rights)",
                "Within five days of first contacting you, a collector must send a written notice of your right to dispute the debt. If you dispute the " +
                "debt in writing within 30 days, the collector must stop collection activity until it provides verification. If you never received that " +
                "notice, or collection continued after your written dispute without verification, note the dates carefully — the Letter Builder can help " +
                "you send a validation request."));
        }

        if (FalseOrInflatedAmounts || AddedUnauthorizedFees)
        {
            Results.Add(new ViolationCard(
                "FCRA — 15 U.S.C. 1681s-2 (furnisher duties)",
                "If the collector reports the debt to the credit bureaus, it acts as a 'furnisher' of information and must not report information it knows " +
                "— or has reasonable cause to believe — is inaccurate. It must also investigate when you dispute through the bureaus. Pull your reports, " +
                "note exactly what is reported, and use the Credit Bureau Dispute template in the Letter Builder if something is wrong."));
        }

        HasResults = Results.Count > 0;
    }
}
