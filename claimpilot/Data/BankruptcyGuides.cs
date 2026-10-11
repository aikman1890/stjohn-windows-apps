using ClaimPilot.Models;

namespace ClaimPilot.Data;

/// <summary>
/// Plain-English bankruptcy guides (Chapter 7 and Chapter 13), document
/// checklists, and standard deadline presets. All original text, written as
/// general information — never legal advice. Every guide urges the user to
/// consider consulting a real bankruptcy attorney.
/// </summary>
public static class BankruptcyGuides
{
    public static BankruptcyGuide GetChapter7Guide() => new(
        "Chapter 7 — Liquidation Bankruptcy",
        "General information about Chapter 7, the 'liquidation' or 'fresh start' bankruptcy for individuals. " +
        "Read the honest notes at the bottom before anything else. Nothing here is legal advice, and nothing here predicts any outcome.",
        new List<BankruptcySection>
        {
            new BankruptcySection(
                "What Chapter 7 is",
                "Chapter 7 is the most common form of personal bankruptcy. A court-appointed trustee gathers your non-exempt property — property the law does not let you keep — sells it, and distributes the proceeds to creditors. " +
                "Most Chapter 7 cases are 'no-asset' cases: the filer's property is all exempt, nothing is sold, and eligible debts are wiped out (discharged) a few months after filing. " +
                "The trade is simple to state and serious to accept: qualifying unsecured debts like credit-card balances, medical bills, and personal loans can be discharged, but the filing stays on your credit report for 10 years and becomes a public record. " +
                "This is general information that MAY apply to your situation — it is not legal advice, and you should strongly consider talking to a bankruptcy attorney before deciding."),

            new BankruptcySection(
                "The means test, explained simply",
                "Not everyone is allowed to file Chapter 7. The means test decides whether your income is low enough. " +
                "Step one: the court compares your average monthly income over the last 6 months (annualized) to the median income for a household of your size in YOUR state. If you are below the median, you pass — the test stops there. " +
                "Step two: if you are above the median, the test subtracts allowed living expenses (housing, food, transport, and other categories the law defines) from your income to see whether you have enough 'disposable income' left to repay creditors. If enough is left, you may be pushed toward Chapter 13 instead. " +
                "The state median figures and allowed-expense amounts change over time — ClaimPilot never hardcodes them because they go stale. Check the current figures for your state, and consider talking to an attorney, because the math has traps for the unwary. " +
                "This is general information, not legal advice."),

            new BankruptcySection(
                "Exempt vs. non-exempt property",
                "Exemptions are the laws that decide what you get to keep. Every state has an exemption scheme, and some states also let you use the federal exemptions — which scheme applies to you depends on where you have lived. " +
                "Typical exemptions protect basic necessities up to dollar limits: a portion of home equity (the homestead exemption), a car up to a value limit, household goods, clothing, tools of your trade, and retirement accounts (which are broadly protected under federal law). " +
                "Property above the exemption limits — a second home, a luxury car with substantial equity, valuable collections — is non-exempt and CAN be taken and sold by the trustee. " +
                "Exemption dollar amounts differ by state and change with inflation adjustments. Do not guess at the numbers: verify the current exemptions for your state, and consider talking to an attorney before you list a single asset. " +
                "This is general information, not legal advice."),

            new BankruptcySection(
                "The 341 meeting of creditors",
                "About 20 to 40 days after you file, you attend the '341 meeting' (named after section 341 of the Bankruptcy Code) — also called the meeting of creditors. " +
                "It is not a courtroom trial and the judge is not there. The trustee swears you in and asks questions about your petition, your income, your assets, and your debts. Creditors are allowed to attend and ask questions, but in most consumer cases none show up. " +
                "You must bring photo ID and proof of your Social Security number. Answer truthfully — lying under oath at a 341 meeting is a federal crime. " +
                "The meeting usually lasts under 15 minutes for a straightforward case. This is general information, not legal advice."),

            new BankruptcySection(
                "Typical Chapter 7 timeline",
                "A routine Chapter 7 case moves fast compared to other bankruptcies: filing day, then the 341 meeting about a month later, then the discharge roughly 60 to 90 days after the meeting — so most cases finish in about 4 to 6 months. " +
                "The automatic stay — the court order that stops most collection calls, lawsuits, garnishments, and foreclosures — takes effect the moment you file, with limited exceptions. " +
                "Two hard gates can stop the discharge: you must complete the pre-filing credit counseling course within 180 days BEFORE filing, and the pre-discharge debtor education course AFTER filing but BEFORE the discharge is entered. Miss the second course and your case can close without a discharge — meaning you went through bankruptcy and your debts survived. " +
                "Use the Required Courses tracker in this app to track both courses, and store your provider-issued certificates in the certificate vault. " +
                "This is general information, not legal advice."),

            new BankruptcySection(
                "Debts that generally are NOT dischargeable",
                "Some debts survive Chapter 7 even when everything goes right. The main categories: domestic support obligations (child support and alimony); most student loans (dischargeable only in narrow hardship cases that are difficult to win); certain recent income taxes; debts obtained by fraud or false pretenses; fines, penalties, and criminal restitution owed to a government; debts for personal injury caused while driving under the influence; and debts you failed to list in your schedules in some situations. " +
                "Secured debts are a separate issue: the lender's lien survives even if your personal obligation is discharged — which is why people in Chapter 7 often reaffirm a car loan or surrender the car. " +
                "Whether a specific debt of yours is dischargeable is exactly the kind of question a bankruptcy attorney answers. This list is general information, not legal advice."),

            new BankruptcySection(
                "Common pitfalls that hurt filers",
                "Hiding assets or lying on the schedules — the trustee and the U.S. Trustee's office look for this, and it can mean losing the discharge or facing federal charges. " +
                "Running up credit-card debt right before filing — recent luxury purchases and cash advances can be ruled non-dischargeable. " +
                "Transferring property to friends or family before filing to 'protect' it — trustees can undo transfers made to dodge creditors. " +
                "Repaying one favored creditor (like a family member) shortly before filing — the trustee can claw it back as a preferential transfer. " +
                "Filing while your income is too high, or timing the filing badly around a bonus, tax refund, or expected inheritance — timing matters and mistakes are expensive. " +
                "This is general information, not legal advice. When in doubt, talk to an attorney BEFORE you move money or sign anything."),
        });

    public static BankruptcyGuide GetChapter13Guide() => new(
        "Chapter 13 — Repayment-Plan Bankruptcy",
        "General information about Chapter 13, the 'wage earner's' bankruptcy built around a 3-to-5-year repayment plan. " +
        "Read the honest notes at the bottom before anything else. Nothing here is legal advice, and nothing here predicts any outcome.",
        new List<BankruptcySection>
        {
            new BankruptcySection(
                "What Chapter 13 is and who it is for",
                "Chapter 13 does not liquidate your property. Instead, you propose a court-approved plan to repay some or all of your debts out of future income over 3 to 5 years, and you keep your property while you pay. " +
                "It is designed for people with regular income. The classic Chapter 13 filer is behind on a mortgage or car loan and needs time to catch up: the plan can cure the arrears over the life of the plan while you resume normal monthly payments — something Chapter 7 cannot do. " +
                "Chapter 13 is also the path for people whose income is too high to pass the Chapter 7 means test, and for people who have non-exempt property they want to keep. " +
                "There are statutory debt limits for Chapter 13 eligibility, and they are adjusted for inflation — verify the current limits before assuming you qualify. " +
                "This is general information that MAY apply to your situation — it is not legal advice, and you should strongly consider talking to a bankruptcy attorney."),

            new BankruptcySection(
                "The 3-to-5-year plan, in plain English",
                "Your plan payment is based on your income, your debts, and what creditors would have received in a Chapter 7 liquidation (the 'best interests of creditors' test — unsecured creditors must get at least what they would have gotten if your non-exempt property had been sold). " +
                "The plan length depends on your income relative to your state's median: below-median filers generally propose 3-year plans; above-median filers are generally committed to 5 years. " +
                "Priority debts — recent taxes, domestic support arrears — must generally be paid in full through the plan. Secured arrears (mortgage, car) are cured over time. Unsecured creditors get whatever the plan provides, which can range from pennies on the dollar to full repayment depending on your income and assets. " +
                "Plan payment math is genuinely complex, and small errors can sink a plan. This summary is general information, not legal advice — an attorney or a careful study of your district's local practices is strongly recommended."),

            new BankruptcySection(
                "How a plan gets confirmed",
                "After filing, you start making plan payments to the Chapter 13 trustee within 30 days — BEFORE the plan is even confirmed. The trustee reviews your plan; creditors can object; and the judge holds a confirmation hearing. " +
                "Common objections: the payment is too low, the plan was not proposed in good faith, or the paperwork is incomplete. Many plans go through one or more rounds of amendments before confirmation. " +
                "Once confirmed, the plan binds you and your creditors: you pay the trustee each month, the trustee pays creditors, and the automatic stay protects you as long as you stay current. " +
                "Local trustee practices and judges' expectations vary significantly by district — what sails through in one courthouse gets objected to in another. This is general information, not legal advice."),

            new BankruptcySection(
                "What happens if you miss plan payments",
                "Missing payments is the most common way Chapter 13 cases die. The trustee can move to dismiss your case, which ends the bankruptcy protection — creditors can resume collection, foreclosure, and lawsuits as if the case never happened. " +
                "Alternatives exist but are narrow: you may be able to modify the plan if your income dropped through no fault of your own, or in some districts cure a short default. In limited hardship situations — such as a serious illness or job loss near the end of a plan — the court can grant a 'hardship discharge' without full plan completion, but the standards are strict. " +
                "Some debtors convert to Chapter 7 instead, if they qualify. Each of these paths has consequences and deadlines. " +
                "If you are falling behind, talk to an attorney immediately — waiting until the dismissal motion is filed limits your options. This is general information, not legal advice."),

            new BankruptcySection(
                "Typical Chapter 13 timeline",
                "Day one: filing and the automatic stay. Within 14 days: most schedules and statements are due. Within 30 days: first plan payment to the trustee. About a month in: the 341 meeting of creditors. Within 45 days after the meeting: the confirmation hearing (often continued). " +
                "Then the long middle: 36 to 60 months of plan payments. Near the end: complete the pre-discharge debtor education course (if not already done), file the required certifications, and receive the discharge — which wipes out the remaining balances on debts the plan covered. " +
                "Most Chapter 13 plans fail before the finish line — the national completion rate is low, which is why attorneys stress budgeting realistically before you file. " +
                "Track every one of these dates in this app's Deadlines section. This is general information, not legal advice."),
        });

    /// <summary>Prominent honest notes shown at the bottom of the Bankruptcy page.</summary>
    public static IReadOnlyList<HonestNote> GetHonestNotes() => new List<HonestNote>
    {
        new HonestNote(
            "Talk to a bankruptcy attorney — this app is not one",
            "Bankruptcy has permanent consequences: a public record, a 10-year mark on your credit report for Chapter 7 (7 years for Chapter 13), possible loss of non-exempt property, and strict deadlines where one mistake can cost you the discharge. " +
            "ClaimPilot provides general information and tracking tools only. It is not a lawyer, it gives no legal advice, it predicts no outcomes, and it files nothing. " +
            "Many bankruptcy attorneys offer free initial consultations, and legal aid offices help qualifying filers at low or no cost. Consider using one before you file anything."),

        new HonestNote(
            "Only U.S. Trustee-approved providers can issue your course certificates",
            "Both required courses — pre-filing credit counseling and pre-discharge debtor education — must come from providers approved by the U.S. Trustee Program. " +
            "Their certificates are generated through the Trustee's Certificate Generation System, numbered and bar-coded against fraud. " +
            "ClaimPilot does NOT issue certificates and cannot satisfy the course requirement. Its tracker only records what YOU completed with a real provider. " +
            "Use the official U.S. Trustee approved-provider lists linked on this page to find a legitimate provider."),

        new HonestNote(
            "Dollar amounts, medians, and exemptions change — verify current figures",
            "Means-test medians, allowed expenses, exemption amounts, and Chapter 13 debt limits are adjusted over time and differ by state. " +
            "ClaimPilot deliberately never hardcodes these numbers, because a stale number is worse than no number. " +
            "Verify every figure against current official sources or with an attorney before relying on it."),

        new HonestNote(
            "Deadlines are case-killers — calendar all of them",
            "Credit counseling must be completed within 180 days BEFORE filing. The debtor education course must be completed BEFORE the discharge — in Chapter 7, cases routinely close without a discharge when filers skip it. " +
            "In Chapter 13, plan payments start within 30 days of filing whether or not the plan is confirmed. " +
            "Add every deadline to the Deadlines section of this app the day you learn it, and treat each one as urgent."),
    };

    /// <summary>Document checklists per chapter — organizational checklists, not legal advice.</summary>
    public static IReadOnlyList<string> GetChapter7Checklist() => new List<string>
    {
        "Credit counseling certificate (completed within 180 days BEFORE filing, from a U.S. Trustee-approved agency)",
        "Pay stubs or other proof of income for the last 60 days",
        "Federal and state tax returns for the last 2 years",
        "Bank statements for the last 3 to 6 months (all accounts)",
        "A complete list of everyone you owe, with amounts, account numbers, and addresses",
        "A complete list of everything you own, with values — real estate, vehicles, bank accounts, household goods, investments",
        "Vehicle titles and current loan statements",
        "Mortgage statements, or your lease if you rent",
        "Retirement and investment account statements",
        "Photo ID and Social Security card (required at the 341 meeting)",
        "Monthly household budget: income and every regular expense",
        "Information about any lawsuits, judgments, or garnishments involving you",
        "Information about any property you sold, transferred, or gave away in the last few years",
        "Divorce decree or support orders, if any",
    };

    public static IReadOnlyList<string> GetChapter13Checklist() => new List<string>
    {
        "Credit counseling certificate (completed within 180 days BEFORE filing, from a U.S. Trustee-approved agency)",
        "Pay stubs or other proof of income for the last 60 days",
        "Federal and state tax returns for the last 4 years (trustees commonly require 4 for Chapter 13)",
        "Bank statements for the last 3 to 6 months (all accounts)",
        "A complete list of everyone you owe, with amounts, account numbers, and addresses",
        "A complete list of everything you own, with values",
        "Mortgage statements showing the exact arrears (missed payments, fees, escrow shortage)",
        "Car loan statements showing payoff amount and any arrears",
        "Vehicle titles and proof of insurance",
        "Photo ID and Social Security card (required at the 341 meeting)",
        "Monthly household budget: income and every regular expense (the plan payment comes from this)",
        "Domestic support obligation details: who you pay, how much, whether you are current",
        "Information about any lawsuits, judgments, or garnishments involving you",
        "Information about any property you sold, transferred, or gave away in the last few years",
        "A realistic proposed plan payment amount you can sustain for 3 to 5 years",
    };

    /// <summary>
    /// Standard bankruptcy deadlines to seed into the Deadlines tracker.
    /// Offsets are relative to a key date the user supplies (filing date or 341 date).
    /// </summary>
    public static IReadOnlyList<(string Title, string Notes, Func<DateTime, DateTime> DueFromFiling)> GetChapter7DeadlinePresets() =>
        new List<(string, string, Func<DateTime, DateTime>)>
        {
            ("Ch 7: Credit counseling must be DONE (within 180 days before filing)",
             "Pre-filing credit counseling from a U.S. Trustee-approved agency must be completed within the 180 days before the filing date. Filing without it risks dismissal.",
             filing => filing),
            ("Ch 7: 341 meeting of creditors (approx. 20-40 days after filing)",
             "Bring photo ID and proof of Social Security number. The trustee will ask about your petition under oath.",
             filing => filing.AddDays(30)),
            ("Ch 7: Reaffirmation agreements due (before discharge)",
             "If you are keeping a car or other secured property by reaffirming the debt, the agreement generally must be filed before the discharge is entered.",
             filing => filing.AddDays(75)),
            ("Ch 7: Debtor education course MUST be completed (before discharge)",
             "The pre-discharge financial management course from a U.S. Trustee-approved provider must be finished before the discharge. Cases close WITHOUT a discharge when this is skipped.",
             filing => filing.AddDays(75)),
            ("Ch 7: Objections to discharge deadline (60 days after first 341 date)",
             "Creditors and the trustee generally have 60 days after the first scheduled 341 meeting to object to discharge. Calendar it from your actual 341 date.",
             filing => filing.AddDays(90)),
        };

    public static IReadOnlyList<(string Title, string Notes, Func<DateTime, DateTime> DueFromFiling)> GetChapter13DeadlinePresets() =>
        new List<(string, string, Func<DateTime, DateTime>)>
        {
            ("Ch 13: Credit counseling must be DONE (within 180 days before filing)",
             "Pre-filing credit counseling from a U.S. Trustee-approved agency must be completed within the 180 days before the filing date. Filing without it risks dismissal.",
             filing => filing),
            ("Ch 13: Schedules and statements due (14 days after filing)",
             "Most remaining schedules, statements, and the plan are due within 14 days of filing if not filed with the petition.",
             filing => filing.AddDays(14)),
            ("Ch 13: First plan payment to trustee (within 30 days of filing)",
             "Plan payments start within 30 days of filing — BEFORE the plan is confirmed. Missing early payments draws trustee scrutiny.",
             filing => filing.AddDays(30)),
            ("Ch 13: 341 meeting of creditors (approx. 20-40 days after filing)",
             "Bring photo ID and proof of Social Security number. The trustee will ask about your petition under oath.",
             filing => filing.AddDays(30)),
            ("Ch 13: Tax returns to trustee (7 days before 341 meeting)",
             "Provide the trustee with copies of required tax returns at least 7 days before the 341 meeting date.",
             filing => filing.AddDays(23)),
            ("Ch 13: Confirmation hearing (approx. 45 days after 341 meeting)",
             "The judge decides whether to confirm your plan. Creditors and the trustee may object first; plans are often amended.",
             filing => filing.AddDays(75)),
            ("Ch 13: Debtor education course MUST be completed (before discharge)",
             "The pre-discharge financial management course from a U.S. Trustee-approved provider must be finished before the discharge at the end of the plan.",
             filing => filing.AddYears(3)),
        };
}
