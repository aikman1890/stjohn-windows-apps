namespace ClaimPilot.Services;

/// <summary>
/// The three complete, professional letter templates. All original text.
/// Each letter is general information, not legal advice, and carries the
/// standard footer note when exported.
/// </summary>
public static class LetterTemplates
{
    public static string BuildDebtValidationLetter(
        string userName, string userAddress,
        string recipientName, string recipientAddress,
        string accountNumber)
    {
        var date = DateTime.Today.ToString("MMMM d, yyyy");
        return
$@"{date}

{recipientName}
{recipientAddress}

Re: Account Number {accountNumber} — Request for Debt Validation under 15 U.S.C. 1692g

To Whom It May Concern:

I am writing in response to your recent communication regarding the above-referenced account. This letter is sent within thirty (30) days of your initial communication with me.

This is not a refusal to pay. It is a request for validation of the alleged debt, as is my right under the Fair Debt Collection Practices Act, 15 U.S.C. 1692g.

Please provide the following:

1. Verification that this debt is mine, including a copy of any signed agreement or contract bearing my signature;

2. A complete accounting of the amount you claim I owe, showing the original principal and separately itemizing every fee, interest charge, and cost you have added, with the contractual or legal basis for each;

3. The name and address of the original creditor, and the name and address of any creditor to whom the debt was sold or assigned before you;

4. Proof that you are licensed to collect debts in my state, if my state requires debt collectors to be licensed;

5. Proof of your authority to collect this debt, including any assignment or purchase agreement showing the chain of ownership from the original creditor to you.

Until you provide the validation requested above, you must cease all collection activity on this account, as required by 15 U.S.C. 1692g(b).

Please direct all future correspondence regarding this matter to me in writing at the address below. I do not consent to telephone calls about this account.

Sincerely,


{userName}
{userAddress}

Enclosures: _______________________________________________

---
This letter was prepared with ClaimPilot. It is general information, not legal advice. Verify everything with your court's rules or an attorney.";
    }

    public static string BuildCeaseCommunicationLetter(
        string userName, string userAddress,
        string recipientName, string recipientAddress,
        string accountNumber)
    {
        var date = DateTime.Today.ToString("MMMM d, yyyy");
        return
$@"{date}

{recipientName}
{recipientAddress}

Re: Account Number {accountNumber} — Direction to Cease Communication under 15 U.S.C. 1692c(c)

To Whom It May Concern:

Pursuant to 15 U.S.C. 1692c(c), I am notifying you in writing to CEASE ALL COMMUNICATION with me regarding the above-referenced account, effective immediately.

I understand the law permits you to contact me only for the following limited purposes: (1) to advise me that your collection efforts are being terminated; (2) to notify me that you may invoke a specified remedy which you ordinarily invoke; or (3) to notify me that you intend to invoke a specified remedy, such as filing a lawsuit.

This direction applies to telephone calls, letters, emails, text messages, and every other form of communication, and it applies to communication with any third party about this alleged debt, subject only to the narrow exceptions the law allows.

This letter does not acknowledge that I owe the debt, waive any of my rights, or extend any deadline. I reserve all rights and remedies available to me under federal and state law, including the Fair Debt Collection Practices Act.

Sincerely,


{userName}
{userAddress}

---
This letter was prepared with ClaimPilot. It is general information, not legal advice. Verify everything with your court's rules or an attorney.";
    }

    public static string BuildBureauDisputeLetter(
        string userName, string userAddress,
        string bureauName, string bureauAddress,
        string accountNumber)
    {
        var date = DateTime.Today.ToString("MMMM d, yyyy");
        return
$@"{date}

{bureauName}
{bureauAddress}

Re: Dispute of inaccurate information — 15 U.S.C. 1681i
Account Number: {accountNumber}

To Whom It May Concern:

I am writing to dispute information appearing on my credit file that is inaccurate or incomplete. Under the Fair Credit Reporting Act, 15 U.S.C. 1681i, I am asking you to investigate the following item(s):

1. Account number {accountNumber}: This item is inaccurate or incomplete because _______________________________________________.
   [Describe what is wrong: for example, wrong balance, account is not mine, paid in full, duplicate entry, or outdated.]

2. [Add additional disputed items here, one per numbered paragraph, identifying each account and what is wrong with it.]

Please conduct a reasonable investigation of this dispute as required by 15 U.S.C. 1681i(a), correct or delete the inaccurate information, and send me written notice of the results as required by 15 U.S.C. 1681i(a)(6).

If you verify the disputed information as accurate, please also provide me with a description of the procedure you used to determine the accuracy and completeness of the information, including the business name, address, and telephone number of any furnisher of information you contacted, as required by 15 U.S.C. 1681i(a)(6)(B)(iii).

Enclosed are copies of documents supporting my dispute. [List enclosures: e.g., copy of government-issued ID, proof of address, account statements, payment records, police report if identity theft.]

Thank you for your prompt attention to this matter.

Sincerely,


{userName}
{userAddress}

Enclosures: _______________________________________________

---
This letter was prepared with ClaimPilot. It is general information, not legal advice. Verify everything with your court's rules or an attorney.";
    }
}
