namespace ClaimPilot.Services;

/// <summary>
/// Central legal-safety copy. The exact disclaimer text below is required by the
/// product spec: it appears in the first-run dialog, on the Dashboard, and on the
/// About page. ClaimPilot never claims to be a lawyer, never predicts outcomes,
/// and never files anything.
/// </summary>
public static class LegalText
{
    public const string Disclaimer =
        "ClaimPilot is not a lawyer and does not give legal advice. " +
        "It provides general legal information and document tools. " +
        "Verify everything with your court's rules or an attorney.";

    public const string MayApplyNote =
        "This MAY apply to the conduct you described. This is general information, not legal advice.";

    public const string GeneralInfoNote =
        "General information only — not legal advice. Consider talking to an attorney.";

    public const string NeverFilesNote =
        "ClaimPilot does not file anything with any court. It organizes information and drafts documents for you to review, verify, and file yourself.";
}
