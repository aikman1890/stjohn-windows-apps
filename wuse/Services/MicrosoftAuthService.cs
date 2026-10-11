using System.Security.Cryptography;
using Microsoft.Identity.Client;
using Windows.Storage;

namespace Wuse.Services;

/// <summary>
/// Microsoft identity via MSAL.NET. Uses the "common" authority so BOTH
/// personal Microsoft accounts (Hotmail/Outlook.com) and work/school accounts
/// (Office 365 / Exchange Online) can sign in, and MULTIPLE accounts can stay
/// connected at once. The MSAL token cache is serialized to a DPAPI-encrypted
/// file (CurrentUser scope) — tokens never sit in plain text on disk.
/// First run opens the Microsoft sign-in page in the default browser.
/// </summary>
public sealed class MicrosoftAuthService
{
    private static readonly string[] Scopes =
    {
        "Mail.Read",
        "Mail.Send",
        "Calendars.Read",
        "Calendars.ReadWrite",
        "offline_access",
        "openid",
        "profile",
    };

    private static readonly string CachePath =
        Path.Combine(ApplicationData.Current.LocalFolder.Path, "wuse-msal-cache.dat");

    private IPublicClientApplication? _app;

    public bool IsConfigured => AppCredentials.IsMicrosoftConfigured;

    private IPublicClientApplication App
    {
        get
        {
            if (_app != null)
                return _app;
            _app = PublicClientApplicationBuilder
                .Create(AppCredentials.MicrosoftClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                .WithDefaultRedirectUri()
                .Build();
            _app.UserTokenCache.SetBeforeAccess(CacheBeforeAccess);
            _app.UserTokenCache.SetAfterAccess(CacheAfterAccess);
            return _app;
        }
    }

    /// <summary>
    /// The connected Microsoft accounts (personal and/or work). Empty when
    /// nobody has signed in yet.
    /// </summary>
    public async Task<IReadOnlyList<IAccount>> GetAccountsAsync()
    {
        if (!IsConfigured)
            return Array.Empty<IAccount>();
        try
        {
            return (await App.GetAccountsAsync()).ToList();
        }
        catch
        {
            return Array.Empty<IAccount>();
        }
    }

    /// <summary>
    /// Adds another Microsoft account (personal or work). Opens the sign-in
    /// page in the browser; the user picks/approves the account there.
    /// </summary>
    public async Task<(bool Ok, string Message)> AddAccountAsync()
    {
        if (!IsConfigured)
            return (false, MicrosoftSetupMessage);
        try
        {
            var result = await App.AcquireTokenInteractive(Scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();
            var label = AccountLabel(result.Account);
            return (true, $"Connected {label} — I'll include it when you ask about email or your calendar.");
        }
        catch (MsalClientException ex) when (ex.ErrorCode == "authentication_canceled")
        {
            return (false, "Sign-in was cancelled — no account was connected. Just say the word when you're ready.");
        }
        catch (Exception ex)
        {
            return (false, $"Microsoft sign-in didn't complete: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets an access token for an account, refreshing silently when possible.
    /// Returns null when interactive sign-in is required (caller should route
    /// the user through AddAccountAsync).
    /// </summary>
    public async Task<string?> GetTokenSilentAsync(IAccount account)
    {
        try
        {
            var result = await App.AcquireTokenSilent(Scopes, account).ExecuteAsync();
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> RemoveAccountAsync(IAccount account)
    {
        try
        {
            await App.RemoveAsync(account);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string AccountLabel(IAccount account)
    {
        var name = account?.Username ?? "Microsoft account";
        var lower = name.ToLowerInvariant();
        if (lower.EndsWith("@outlook.com") || lower.EndsWith("@hotmail.com") || lower.EndsWith("@live.com"))
            return $"{name} (Outlook)";
        return $"{name} (Office 365)";
    }

    public const string MicrosoftSetupMessage =
        "Microsoft isn't set up yet — a client ID from an Entra app registration needs " +
        "to go into the app's AppCredentials.cs (one-time setup, the comments there walk " +
        "through it — it covers both personal Hotmail/Outlook.com and work Office 365 " +
        "accounts). Once that's in, I'll open the Microsoft sign-in page for you.";

    // ---------- DPAPI token-cache persistence ----------

    private static void CacheBeforeAccess(TokenCacheNotificationArgs args)
    {
        try
        {
            if (File.Exists(CachePath))
            {
                var protectedBytes = File.ReadAllBytes(CachePath);
                var plain = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                args.TokenCache.DeserializeMsalV3(plain);
            }
        }
        catch
        {
            // Corrupt cache: start fresh rather than break sign-in.
        }
    }

    private static void CacheAfterAccess(TokenCacheNotificationArgs args)
    {
        if (!args.HasStateChanged)
            return;
        try
        {
            var plain = args.TokenCache.SerializeMsalV3();
            var protectedBytes = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(CachePath, protectedBytes);
        }
        catch
        {
            // Cache persistence is best-effort; the session still works.
        }
    }

}
