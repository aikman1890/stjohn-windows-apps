using ClaimPilot.Models;

namespace ClaimPilot.Data;

/// <summary>
/// Plain-English self-help content for the Legal Matters add-on. All original
/// text. General information only — never legal advice, never a prediction.
/// Where the law varies by state or court, the text says so and points the
/// reader to their court's rules or an attorney.
/// </summary>
public static class LegalMattersContent
{
    public static IReadOnlyList<GuideSection> GetTrafficTicketSections() => new List<GuideSection>
    {
        new GuideSection(
            "How a traffic ticket works",
            "A traffic ticket (citation) is a written allegation that you broke a traffic law. The officer is not the judge — " +
            "the ticket is the start of a court process, not the verdict. Your ticket should list a court name and either a court " +
            "date or a deadline to respond. Read the whole ticket, front and back. Ignoring a ticket is the worst option: missed " +
            "deadlines can lead to extra fines, a warrant for your arrest, or a suspended driver's license, depending on your state. " +
            "If you lost the ticket, call the court listed on it — or the court for the city or county where you were stopped."),

        new GuideSection(
            "Your options",
            "Most courts give you a few paths. Paying the fine is usually the simplest, but in many places paying counts as an " +
            "admission or conviction, which can add points to your driving record and raise your insurance. Contesting the ticket " +
            "means going to court and asking the judge or jury to decide — you can represent yourself or hire a traffic attorney. " +
            "Many courts also offer middle paths: defensive driving courses, deferred disposition (the ticket is dismissed if you " +
            "meet conditions like no new tickets for a set period), or dismissal programs. These vary widely by court, so ask the " +
            "clerk what options your court offers. Clerks can explain procedures; they cannot give legal advice."),

        new GuideSection(
            "Check the ticket itself",
            "Before your court date, review the ticket calmly for mistakes: your name, the date, time, and location of the stop, " +
            "vehicle information, the violation written down, and the officer's name or badge number. Small errors do not always " +
            "get a ticket dismissed, but real errors in the key facts can matter — write them down and raise them in court, not on " +
            "the roadside. If the ticket was for speeding, the officer's speed-measuring device (radar or lidar) generally needs " +
            "regular calibration and maintenance; in many courts you can ask about calibration records through the court's process. " +
            "Never argue with the officer during the stop — be polite, sign the ticket if required (signing is usually just a promise " +
            "to appear, not an admission), and save your arguments for court."),

        new GuideSection(
            "The municipal court process",
            "Most traffic tickets go to municipal court, justice court, or a similar local court — the name varies by state. " +
            "A typical path: arraignment (you enter a plea of guilty, not guilty, or no contest), then possibly a pre-trial " +
            "conference, then trial if you contest. Trials for tickets are often short and informal compared to higher courts, " +
            "but the same basics apply: the government must prove the violation, and you can question witnesses and present your " +
            "side. Some courts let you resolve tickets by mail or online — check your court's website or call the clerk."),

        new GuideSection(
            "Hearing prep checklist",
            "Use this checklist before your court date:\n" +
            "• Read the ticket front and back and note your deadline or court date.\n" +
            "• Decide your plea: pay, contest, or ask about driving school or deferred options.\n" +
            "• Gather documents: driver's license, proof of insurance, vehicle registration, and any photos of the scene, signs, or road conditions.\n" +
            "• Write a short, factual timeline of what happened — dates, times, what you saw.\n" +
            "• Arrive early, dress respectfully, and address the judge as \"Your Honor.\"\n" +
            "• Be brief and factual. Do not interrupt the judge, the officer, or witnesses.\n" +
            "• Bring a pen and paper to write down what the judge orders.\n" +
            "• If anything is unclear, ask the clerk about procedures before your hearing — not legal advice, just how things work."),
    };

    public static IReadOnlyList<GuideSection> GetCriminalCaseSections() => new List<GuideSection>
    {
        new GuideSection(
            "Your rights",
            "If you are questioned or arrested, you have important rights. Under Miranda v. Arizona, before a custodial " +
            "interrogation police must warn you that you have the right to remain silent, that anything you say can be used " +
            "against you, that you have the right to an attorney, and that if you cannot afford one, one will be appointed for " +
            "you (a public defender). You can invoke these rights at any time by saying clearly: \"I am remaining silent. I want " +
            "a lawyer.\" You generally do not have to answer police questions, and you can ask, \"Am I free to leave?\" Do not " +
            "physically resist an arrest even if you believe it is unjustified — resisting can add charges. Contest the arrest " +
            "later, in court, with a lawyer."),

        new GuideSection(
            "Misdemeanor vs. felony",
            "Crimes are generally grouped by seriousness. Misdemeanors are less serious offenses — typically punishable by up to " +
            "a year in a local or county jail, fines, probation, or community service. Felonies are more serious — punishable by " +
            "a year or more, usually in state prison, plus larger fines. The exact lines, labels, and punishments vary by state, " +
            "and some offenses can be charged either way. A felony conviction can also affect voting rights, gun rights, housing, " +
            "and employment long after the sentence ends — another reason to take any criminal charge seriously and get a lawyer."),

        new GuideSection(
            "Stages of a criminal case",
            "A criminal case generally moves through these stages, though the names and order vary by state:\n" +
            "• Arrest and booking — you are taken into custody and processed.\n" +
            "• Arraignment — the charges are read and you enter a plea (guilty, not guilty, or no contest).\n" +
            "• Bail or bond hearing — the court decides conditions for your release while the case is pending.\n" +
            "• Discovery — both sides exchange evidence.\n" +
            "• Pre-trial motions and plea negotiations — your lawyer can challenge evidence and discuss possible plea deals with the prosecutor.\n" +
            "• Trial — before a judge or jury, where the prosecution must prove guilt beyond a reasonable doubt.\n" +
            "• Sentencing — if convicted, the judge sets the punishment within the law's range.\n" +
            "Most criminal cases end in a plea agreement, not a trial — but you should never accept a plea without a lawyer's advice."),

        new GuideSection(
            "Bail basics",
            "Bail (or bond) is money or conditions set to make sure you show up for court. It may be set by a bail schedule or " +
            "by a judge at a hearing, based on the charge, your record, and whether you are a flight risk. Options can include " +
            "cash bail, a bail bond through a bondsman (usually a non-refundable fee), or release on your own recognizance with " +
            "conditions like check-ins or travel limits. If you cannot afford bail, tell your attorney immediately — many places " +
            "have procedures to ask for lower bail or non-monetary release. Missing a court date can mean a warrant and losing " +
            "whatever was posted."),

        new GuideSection(
            "What to do — and not do — after an arrest",
            "DO:\n" +
            "• Stay calm and be polite. Give your name if required, then stop talking.\n" +
            "• Ask for a lawyer clearly and repeatedly. Say nothing about the case until one arrives.\n" +
            "• Remember officer names, badge numbers, times, and what was said — write it all down as soon as you can.\n" +
            "• Tell your lawyer everything, truthfully. They cannot defend what they do not know.\n" +
            "• Show up to every court date, early and dressed respectfully.\n\n" +
            "DO NOT:\n" +
            "• Discuss your case with anyone except your lawyer — jail phone calls are recorded, and cellmates can testify.\n" +
            "• Post about the case on social media, by text, or by email.\n" +
            "• Contact the alleged victim or witnesses.\n" +
            "• Miss court dates or violate release conditions.\n" +
            "• Sign anything or agree to anything without your lawyer."),
    };

    public static IReadOnlyList<GuideSection> GetFederalMatterSections() => new List<GuideSection>
    {
        new GuideSection(
            "State vs. federal court",
            "Most legal cases live in state court. Federal courts handle a narrower set: cases involving federal law or the U.S. " +
            "Constitution, cases where the parties are from different states and enough money is at stake (\"diversity\" cases), " +
            "and prosecutions for violations of federal criminal law. The trial-level federal courts are the U.S. district courts — " +
            "there are 94 of them, and ClaimPilot's Court Finder can help you locate yours. Above them are the 13 U.S. Courts of " +
            "Appeals, and above those the U.S. Supreme Court. Federal court has its own rules, its own pace, and generally higher " +
            "stakes — get a lawyer experienced in federal practice."),

        new GuideSection(
            "How federal cases start",
            "A federal civil case generally starts when the plaintiff files a complaint in a U.S. district court, pays the filing " +
            "fee (or asks to proceed without prepaying it), and serves the defendant. A federal criminal case generally starts " +
            "with an investigation by a federal agency, followed by a complaint, an indictment voted by a grand jury, or an " +
            "\"information\" filed by the prosecutor. Federal prosecutors have vast resources — anyone facing federal investigation " +
            "or charges should get a federal criminal defense attorney immediately and say nothing to investigators without one."),

        new GuideSection(
            "The federal criminal process, at a high level",
            "After arrest or indictment: an initial appearance before a magistrate judge (charges explained, counsel addressed, " +
            "detention or release decided), then either a preliminary hearing or grand jury proceedings, then arraignment on the " +
            "indictment, then discovery and pre-trial motions, then plea negotiations or trial, then sentencing. At sentencing, " +
            "judges consult the U.S. Sentencing Guidelines — advisory ranges based on the offense and the defendant's history. " +
            "Federal sentences can be severe and are served in the federal prison system. This is a simplified overview; federal " +
            "criminal procedure is technical and the details matter enormously."),

        new GuideSection(
            "Federal civil basics",
            "Federal civil cases follow the Federal Rules of Civil Procedure: complaint, service of process, the defendant's " +
            "answer, discovery (often extensive and expensive), motion practice — including motions to dismiss and motions for " +
            "summary judgment — and then trial or settlement. Deadlines in federal court are strict and usually shorter than " +
            "people expect. Many federal civil-rights cases against state actors are filed under 42 U.S.C. 1983 — see the Civil " +
            "Rights section of this app for plain-English guides, including honest notes on defenses like qualified immunity."),

        new GuideSection(
            "Find your district court",
            "Use the Court Finder section of this app to locate any of the 94 U.S. district courts, with links to each court's " +
            "official website. Court websites publish local rules, forms, fee schedules, and pro se handbooks — read the local " +
            "rules of the specific court before drafting or filing anything. When in doubt, call the clerk's office: clerks can " +
            "explain procedures, though they cannot give legal advice."),
    };

    public static IReadOnlyList<GuideSection> GetCourtGuideSections() => new List<GuideSection>
    {
        new GuideSection(
            "Which court handles what",
            "Courts are divided by the kinds of cases they can hear:\n" +
            "• Municipal / city court — traffic tickets, city ordinance violations, and sometimes low-level misdemeanors.\n" +
            "• Justice of the peace — small claims, evictions, and minor misdemeanors in many states (names vary).\n" +
            "• County court — misdemeanors and civil cases up to a dollar limit set by state law.\n" +
            "• District / superior / circuit court — felonies, larger civil cases, and family law matters. This is the main trial court in most states.\n" +
            "• Federal district court — cases under federal law, constitutional claims, and certain cases between parties from different states.\n" +
            "• Appeals courts — review decisions of lower courts; they generally do not retry facts.\n" +
            "Filing in the wrong court wastes time and filing fees — when in doubt, ask the clerk which court handles your kind of case."),

        new GuideSection(
            "How to find your court",
            "Your ticket, citation, or court paperwork should name the court. From there: search the court's official website for " +
            "its address, hours, phone number, and local rules. Most courts publish filing information, forms, and fee schedules " +
            "online. If you are not sure which court you need, call the clerk's office of the most likely court and ask — clerks " +
            "answer procedure questions every day. For federal courts, use this app's Court Finder."),

        new GuideSection(
            "Filing basics",
            "Most filings start with a form — get it from the clerk's office or the court's website rather than inventing your " +
            "own. Filing fees apply to most civil filings; the amounts change, so check the court's current fee schedule (this app " +
            "never lists fee amounts). Make copies: one for the court, one for each other party, and one for yourself. After filing, " +
            "the other side generally must be formally notified — \"service of process\" — and courts are strict about proper service. " +
            "Keep a copy of everything you file, stamped by the clerk, in one organized folder."),

        new GuideSection(
            "What to bring",
            "For any court appearance, bring: a photo ID; every piece of paperwork related to your case, organized in a folder; " +
            "copies for the judge and the other side if you plan to hand anything up; your evidence (photos, receipts, contracts); " +
            "a pen and paper for notes; and a way to pay any fees or fines. Leave extra time for parking and security screening. " +
            "Bring the patience to wait — court dockets run on the court's schedule, not yours."),

        new GuideSection(
            "Courtroom etiquette",
            "• Arrive early and check in with the clerk or bailiff if instructed.\n" +
            "• Dress respectfully — business-casual at minimum; no hats, sunglasses, shorts, or flip-flops.\n" +
            "• Stand when the judge enters or leaves. Address the judge as \"Your Honor.\"\n" +
            "• Never interrupt the judge, the other side, or a witness. You will get your turn.\n" +
            "• Silence your phone completely. No food, gum, or drinks.\n" +
            "• Speak clearly into the microphone if there is one; the hearing is being recorded.\n" +
            "• Be brief and stick to facts. Arguing with the judge never helps."),
    };
}
