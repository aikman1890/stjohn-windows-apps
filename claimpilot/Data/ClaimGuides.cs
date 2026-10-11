using System.Collections.ObjectModel;
using ClaimPilot.Models;

namespace ClaimPilot.Data;

/// <summary>
/// Plain-English guides to six common federal civil-rights claim types under
/// 42 U.S.C. 1983. All original text. Every guide uses "MAY apply" language,
/// explains qualified immunity honestly, and is labeled general information —
/// never legal advice, never a prediction about any case.
/// </summary>
public static class ClaimGuides
{
    public static IReadOnlyList<ClaimGuide> GetGuides() => new List<ClaimGuide>
    {
        new ClaimGuide(
            "Excessive Force",
            "What it is: This claim says a law-enforcement officer used more physical force than was reasonably necessary under the circumstances. " +
            "It explicitly covers NON-LETHAL force: beatings, punches and knee strikes, misuse of a taser, chokeholds, and police dog bites, as well as shootings. " +
            "The force is judged from the perspective of a reasonable officer on the scene, in a tense and fast-moving situation — not with the benefit of hindsight. " +
            "Courts weigh the factors from Graham v. Connor: how serious the suspected offense was, whether the person posed an immediate threat, and whether the person was resisting arrest or trying to flee. " +
            "You do not need a life-threatening injury to bring this claim, but the severity of your injuries matters a great deal to what a case may be worth. " +
            "This is general information that MAY apply to force you experienced — it is not legal advice, and you should consider talking to an attorney.",
            "What a plaintiff must generally show: First, that the person who used force was acting under color of state law — typically a state or local officer on duty. " +
            "Second, that you were seized — for example, stopped, arrested, or otherwise not free to leave. " +
            "Third, that the amount of force used was objectively unreasonable in light of the facts the officer faced at the time. " +
            "Fourth, that the unreasonable force caused you injury. " +
            "You will need evidence tying a specific officer to the specific force used — vague claims against 'the police' usually fail. " +
            "Medical records, photos, video, and witness statements are often the backbone of this kind of case. " +
            "This describes what plaintiffs generally must show; it is not a prediction about any case.",
            "Common defenses: The most common and most powerful defense is qualified immunity: an officer may avoid liability if the constitutional right was not 'clearly established' at the time — " +
            "meaning earlier cases had not put officers on notice that this exact kind of force was unlawful. This defense ends many cases before trial, and you should understand that honestly. " +
            "Officers also argue the force was justified by a threat, by resistance, or by an attempt to flee. " +
            "They may argue the force was too minor ('de minimis') to violate the Constitution at all. " +
            "If you cannot identify which officer used the force, the case may fail for that reason alone. " +
            "And under Heck v. Humphrey, if winning your civil case would necessarily imply that your criminal conviction was invalid, the civil case may be barred while the conviction stands."),

        new ClaimGuide(
            "False Arrest",
            "What it is: A false-arrest claim says you were arrested or detained without probable cause or other legal authority, in violation of the Fourth Amendment. " +
            "An arrest happens when a reasonable person would not feel free to leave — it does not require handcuffs or a trip to the station. " +
            "The central question is whether the officer had probable cause: facts and circumstances, known to the officer at the time, that would lead a reasonable officer to believe you had committed a crime. " +
            "An arrest can be unlawful even if you were later released without charges, and even if the officer acted politely. " +
            "What the officer learned after the arrest generally does not matter; what matters is what was known when the arrest was made. " +
            "This is general information that MAY apply to your situation — not legal advice and not a conclusion about any case.",
            "What a plaintiff must generally show: You must show that a person acting under color of state law seized you — arrested or detained you — and that the seizure was made without probable cause. " +
            "You must show what the officer knew at the time and why those facts did not add up to probable cause for a reasonable officer. " +
            "It helps to establish the absence of a warrant, the absence of consent, and the absence of any observed criminal conduct. " +
            "You must also show harm — even a short unlawful detention counts, though damages vary widely from case to case. " +
            "Keep the focus on the facts known to the officer when the arrest was made, not on what anyone learned later. " +
            "Consider talking to an attorney before drawing conclusions from this summary.",
            "Common defenses: Officers most often argue they did have probable cause — and courts ask only whether a reasonable officer could have believed so, not whether the officer was ultimately right. " +
            "Under Devenpeck v. Alford, probable cause for any offense can justify the arrest, even if it was not the offense the officer named at the time. " +
            "Officers also invoke qualified immunity, arguing the law did not clearly establish that these facts lacked probable cause — this defense ends many cases before trial. " +
            "A valid arrest warrant, or your genuine consent to the encounter, can also defeat the claim. " +
            "If charges were filed and you were convicted, the Heck rule may block a civil claim that would imply the conviction was wrong."),

        new ClaimGuide(
            "Malicious Prosecution",
            "What it is: A malicious-prosecution claim says criminal proceedings were started or continued against you without probable cause and for an improper purpose. " +
            "After Thompson v. Clark, the federal claim is framed as a Fourth Amendment claim: you were seized under legal process — an arrest warrant, an indictment, or an arraignment — without probable cause. " +
            "It is not enough that you were investigated or questioned; a criminal proceeding must actually have been initiated. " +
            "The proceedings must have ended in your favor, such as an acquittal, a dismissal, or a nolle prosequi — though not every dismissal counts, as explained below. " +
            "This is a technical claim with fine distinctions. This is general information that MAY apply — not legal advice.",
            "What a plaintiff must generally show: You must show that someone acting under color of state law caused criminal proceedings to be instituted against you. " +
            "You must show those proceedings resulted in a seizure under legal process, such as an arrest on a warrant or detention after arraignment. " +
            "You must show the proceedings were pursued without probable cause. " +
            "You must show malice or an improper purpose behind the prosecution. " +
            "Critically, you must show the proceedings terminated in your favor — and the exact reason for the termination matters, because compromise dismissals often do not count. " +
            "You must also show damages flowing from the prosecution, such as detention, legal costs, or reputational harm. " +
            "Because this claim turns on fine distinctions, consider talking to an attorney before relying on this summary.",
            "Common defenses: The strongest defense is probable cause — if probable cause existed, the claim generally fails even if the motive behind the prosecution was bad. " +
            "Defendants argue the termination was not truly favorable: a dismissal as part of a plea deal or other compromise usually does not count as favorable. " +
            "Under Heck v. Humphrey, an outstanding conviction bars a claim that would imply the conviction is invalid. " +
            "Prosecutors themselves have absolute immunity for their prosecutorial acts, so these claims are usually brought against officers or complaining witnesses, not the prosecutor. " +
            "And qualified immunity — the argument that the right was not 'clearly established' in these circumstances — ends many cases before trial."),

        new ClaimGuide(
            "Failure to Intervene",
            "What it is: Officers who are present when a fellow officer violates someone's constitutional rights may be liable if they fail to step in. " +
            "This is the failure-to-intervene claim (sometimes called failure to protect). " +
            "It most often arises alongside excessive-force claims: one officer uses unreasonable force while others watch and do nothing. " +
            "The duty applies to officers who observed the violation or had reason to know it was happening — it does not require the observing officer to have used any force themselves. " +
            "Bystander officers, supervisors on scene, and even officers from other agencies who were present can face this claim. " +
            "This is general information that MAY apply to what you witnessed or experienced — not legal advice.",
            "What a plaintiff must generally show: You must show that an officer acting under color of state law knew, or had reason to know, that a constitutional violation — such as excessive force — was occurring or about to occur. " +
            "You must show the officer had a realistic opportunity to intervene, meaning the time and ability to act. " +
            "You must show the officer failed to take reasonable steps to stop the violation, such as verbal commands, physical interposition, or calling for a supervisor. " +
            "And you must show the underlying violation actually occurred — without it, there is nothing the officer failed to stop. " +
            "Evidence of who was standing where, for how long, and what each person could see is often decisive. " +
            "Consider talking to an attorney about how these facts fit together.",
            "Common defenses: Officers argue they had no realistic opportunity to intervene — that events unfolded in seconds with no chance to act. " +
            "They argue they did not see what was happening or did not understand it as a constitutional violation in the moment. " +
            "They argue the underlying force was lawful, so there was nothing to stop. " +
            "And qualified immunity applies here too: if the law did not clearly establish a duty to intervene in these circumstances, the defense can end the case before trial. " +
            "Supervisors who were not present may separately argue they were not personally involved in the incident."),

        new ClaimGuide(
            "Deliberate Indifference",
            "What it is: Jail and prison officials must not be deliberately indifferent to the serious medical needs of people in custody, or to conditions that pose a substantial risk of serious harm. " +
            "For convicted prisoners the claim comes from the Eighth Amendment's ban on cruel and unusual punishment; for pretrial detainees it comes from the Fourteenth Amendment's due-process protections. " +
            "Examples include ignoring repeated pleas for medical help, denying prescribed medication, delaying care for an obviously serious condition, or housing a vulnerable person with a known violent threat. " +
            "Simple negligence or medical malpractice is not enough — the standard is deliberately higher than carelessness. " +
            "This is general information that MAY apply — not legal advice, and not a conclusion about any case.",
            "What a plaintiff must generally show: You must show an objectively serious medical need or a substantial risk of serious harm — something a reasonable person would recognize as needing attention. " +
            "You must show the official actually knew of the risk and deliberately disregarded it — this subjective knowledge requirement is the heart of the claim and is harder to prove than mere carelessness. " +
            "You must show the disregard caused your injury, not just that you were unhappy with the care. " +
            "Grievances, medical request slips, sick-call logs, and witness accounts help prove what officials knew and when they knew it. " +
            "For pretrial detainees, some federal circuits apply a more plaintiff-friendly objective test — the law varies by circuit, so verify your circuit's rule. " +
            "Consider talking to an attorney before drawing conclusions from this summary.",
            "Common defenses: The most common defense is that the official was at most negligent — that a mistake, a delay, or a disagreement about treatment is malpractice, not deliberate indifference. " +
            "Officials argue they did not actually know about the risk, attacking the subjective-knowledge requirement directly. " +
            "They argue they responded reasonably once they knew — courts give officials latitude in choosing how to respond. " +
            "Qualified immunity — the argument that the right was not 'clearly established' in these circumstances — ends many cases before trial. " +
            "Claims based on a mere difference of medical opinion, or against private medical contractors, face additional hurdles."),

        new ClaimGuide(
            "Monell Municipal Liability (Suing the City or County)",
            "What it is: Under Monell v. Department of Social Services, a city, county, or other municipality can be held liable when its own policy or custom causes a constitutional violation. " +
            "This is how you sue 'the city' rather than just the individual officer. " +
            "Liability can rest on an official written policy, a widespread custom or practice that policymakers tolerated, or a failure to train or supervise that reflects deliberate indifference to constitutional rights. " +
            "A city is not liable just because it employs someone who did wrong — there is no respondeat superior (automatic employer liability) under Section 1983. " +
            "States and state agencies generally cannot be sued this way because of Eleventh Amendment immunity; Monell reaches municipalities and counties. " +
            "This is general information that MAY apply — not legal advice.",
            "What a plaintiff must generally show: You must show an underlying constitutional violation by a municipal employee — without it, the municipal claim usually collapses. " +
            "You must identify the municipal policy, custom, or failure to train, and connect it to a final policymaker for the city or county. " +
            "For failure-to-train claims, you must show deliberate indifference: that policymakers knew officers would face a recurring situation and chose not to train for it anyway. " +
            "Finally, you must show the policy or failure was the 'moving force' behind your injury — a direct causal link, not just background context. " +
            "Pattern evidence, such as prior similar incidents the city ignored, is often essential. " +
            "This is a demanding claim; consider talking to an attorney.",
            "Common defenses: Cities argue there was no official policy or custom — just one employee's rogue act, which is not enough under Monell. " +
            "They argue the training evidence shows at most negligence, not deliberate indifference. " +
            "They argue a single incident cannot prove deliberate indifference, with only narrow exceptions. " +
            "They dispute that the official you identified was actually a final policymaker for the municipality. " +
            "If the underlying constitutional claim against the officer fails, the Monell claim usually fails with it. " +
            "Qualified immunity does not protect municipalities, but the demanding proof requirements above defeat many Monell claims before trial — an honest reality to understand."),
    };

    /// <summary>
    /// The three prominent honest-notes shown at the top of the Civil Rights page.
    /// </summary>
    public static IReadOnlyList<HonestNote> GetHonestNotes() => new List<HonestNote>
    {
        new HonestNote(
            "Time limits borrow state law — verify YOUR state's deadline",
            "Section 1983 has no federal statute of limitations of its own. Courts borrow the state's personal-injury limitations period, which VARIES by state — commonly between 1 and 6 years. " +
            "ClaimPilot never states a universal deadline because there isn't one. Look up your state's period immediately, and consider talking to an attorney — filing even one day late can end the case."),

        new HonestNote(
            "Section 1983 requires 'under color of state law' — federal actors are different",
            "Section 1983 only reaches people acting 'under color of state law' — state and local officials and those working with them. " +
            "It generally does NOT reach federal officers or federal agencies. Claims against federal actors use a different theory called a Bivens claim, " +
            "which courts have narrowed sharply in recent years and which is much harder to win. Suing the wrong kind of defendant under the wrong theory is a common, case-ending mistake."),

        new HonestNote(
            "Deadlines can be brutally short — missing one can end the case",
            "Statutes of limitations are only the start. Notice-of-claim rules (some as short as weeks or a few months), grievance-exhaustion requirements for prisoners, " +
            "and court scheduling orders can all end a case if missed. Treat every deadline as urgent: check it immediately, calendar it in the Deadlines section of this app, " +
            "and consider talking to an attorney right away rather than waiting."),
    };
}
