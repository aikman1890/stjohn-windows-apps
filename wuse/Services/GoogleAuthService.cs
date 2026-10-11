using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace Wuse.Services;

/// <summary>
/// Google OAuth 2.0 desktop-app flow (loopback redirect: the sign-in page
/// opens in the user's default browser on first run). Tokens persist in a
/// DPAPI-encrypted file via <see cref="DpapiDataStore"/> — never plain JSON.
/// </summary>
public sealed class GoogleAuthService
{
    private static readonly string[] Scopes =
    {
        GmailService.Scope.GmailModify,   // read + send mail
        CalendarService.Scope.Calendar,   // read + write calendar
    };

    private UserCredential? _credential;

    public bool IsConfigured => AppCredentials.IsGoogleConfigured;

    public async Task<(bool Ok, string Message, UserCredential? Credential)> EnsureCredentialAsync()
    {
        if (_credential != null)
            return (true, string.Empty, _credential);

        if (!IsConfigured)
            return (false,
                "Google isn't set up yet — a Google client ID and secret need to go into the " +
                "app's AppCredentials.cs (one-time setup, see the comments there). " +
                "Once that's in, I'll open the Google sign-in page for you.",
                null);

        try
        {
            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = AppCredentials.GoogleClientId,
                    ClientSecret = AppCredentials.GoogleClientSecret,
                },
                Scopes,
                "user",
                CancellationToken.None,
                new DpapiDataStore("wuse-google-tokens.dat"));
            return (true, string.Empty, _credential);
        }
        catch (Exception ex)
        {
            return (false, $"Google sign-in didn't complete: {ex.Message}", null);
        }
    }

    public void SignOut()
    {
        _credential = null;
        try { File.Delete(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "wuse-google-tokens.dat")); }
        catch { }
    }
}
