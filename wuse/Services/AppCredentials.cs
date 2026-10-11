namespace Wuse.Services;

/// <summary>
/// OAuth client registrations for wuse's email + calendar integrations.
/// NO secrets are hardcoded here — Shaun (or the person deploying wuse)
/// pastes their own client IDs below, once. Until then, the email/calendar
/// commands gracefully explain what's missing instead of failing.
/// </summary>
public static class AppCredentials
{
    // ================= GOOGLE (Gmail + Google Calendar) =================
    // Setup (one time):
    //   1. https://console.cloud.google.com → create/select a project.
    //   2. "APIs & Services → Library": enable "Gmail API" and "Google Calendar API".
    //   3. "APIs & Services → OAuth consent screen": choose External, fill the
    //      app name, add your Gmail address as a test user.
    //   4. "APIs & Services → Credentials → Create Credentials → OAuth client ID
    //      → Application type: Desktop app". Copy the client ID and secret here.
    public const string GoogleClientId = "PASTE-YOUR-GOOGLE-CLIENT-ID-HERE.apps.googleusercontent.com";
    public const string GoogleClientSecret = "PASTE-YOUR-GOOGLE-CLIENT-SECRET-HERE";

    public static bool IsGoogleConfigured =>
        !GoogleClientId.StartsWith("PASTE-", StringComparison.Ordinal) &&
        !GoogleClientSecret.StartsWith("PASTE-", StringComparison.Ordinal);

    // ============ MICROSOFT (Outlook / Hotmail / Office 365 via Graph) ============
    // Setup (one time):
    //   1. https://portal.azure.com → Microsoft Entra ID → App registrations
    //      → New registration.
    //   2. "Supported account types": choose "Accounts in any organizational
    //      directory AND personal Microsoft accounts" (this covers Hotmail /
    //      Outlook.com AND Office 365 work/school accounts).
    //   3. Redirect URI: platform "Public client/native (mobile & desktop)",
    //      value http://localhost
    //   4. "API permissions → Add a permission → Microsoft Graph → Delegated":
    //      add Mail.Read, Mail.Send, Calendars.Read, Calendars.ReadWrite.
    //   5. Copy the "Application (client) ID" here. No client secret is needed
    //      (public/native client).
    public const string MicrosoftClientId = "PASTE-YOUR-MICROSOFT-CLIENT-ID-HERE";

    public static bool IsMicrosoftConfigured =>
        !MicrosoftClientId.StartsWith("PASTE-", StringComparison.Ordinal);
}
