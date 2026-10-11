using ClaimPilot.Licensing;

namespace ClaimPilot.Services;

/// <summary>
/// Bankruptcy Plus add-on licensing. Direct-sale build: entitlement comes
/// from an offline HMAC-signed license key ($9.99/month), NOT the Microsoft
/// Store. The creator unlock still short-circuits everything on Shaun's
/// own machines (see <see cref="CreatorUnlockService"/>).
///
/// Fully offline: no network calls, no telemetry.
/// </summary>
public static class PurchaseService
{
    /// <summary>
    /// True when the user holds a valid Bankruptcy Plus key — or when the
    /// local Creator Unlock is active.
    /// </summary>
    public static Task<bool> IsBankruptcyPlusOwnedAsync()
    {
        // Creator Unlock short-circuits the key check: local, PIN-gated,
        // documented in BUILD.md. Not a bypass for anyone else.
        if (AppLicense.CreatorUnlocked)
        {
            return Task.FromResult(true);
        }

        try
        {
            return Task.FromResult(AppLicense.Plus.HasValidKey());
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Activates a Bankruptcy Plus key the user typed. Returns success flag
    /// and a human-readable message for the UI.
    /// </summary>
    public static Task<(bool Success, string Message)> ActivateBankruptcyPlusAsync(string keyText, string email)
    {
        try
        {
            if (AppLicense.Plus.TryActivate(keyText, email, out var error))
            {
                return Task.FromResult((true, "Bankruptcy Plus unlocked. The bankruptcy modules are now available."));
            }
            return Task.FromResult((false, error));
        }
        catch (Exception ex)
        {
            return Task.FromResult((false, "Activation failed: " + ex.Message));
        }
    }

    /// <summary>
    /// Legacy Store purchase entry point. The direct-sale build has no Store
    /// checkout; keys are issued by KeyGen and emailed to buyers. Kept for
    /// API compatibility — always directs the user to key activation.
    /// </summary>
    public static Task<(bool Success, string Message)> PurchaseBankruptcyPlusAsync()
    {
        return Task.FromResult((false,
            "Purchases are handled by email in this build. Write to zerocoolok1983@gmail.com " +
            "for a Bankruptcy Plus key, then enter it below to unlock."));
    }
}
