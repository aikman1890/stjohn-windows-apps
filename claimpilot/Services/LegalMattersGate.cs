using ClaimPilot.Licensing;

namespace ClaimPilot.Services;

/// <summary>
/// Gate for the Legal Matters add-on ($25.99/month): traffic tickets, criminal
/// cases, federal matters, court guides, and the case builder.
///
/// Unlocked when EITHER:
///   - the 7-tap creator unlock is active (Shaun's own machines), or
///   - a valid, unexpired Legal Matters license key is stored on this PC.
///
/// Fully offline: HMAC-signed keys verified locally, no network calls.
/// </summary>
public static class LegalMattersGate
{
    /// <summary>True when the user holds a valid Legal Matters add-on license.</summary>
    public static bool IsUnlocked()
    {
        if (AppLicense.CreatorUnlocked)
            return true;
        try
        {
            return AppLicense.LegalMatters.HasValidKey();
        }
        catch
        {
            return false;
        }
    }

    public const string AddOnName = "Legal Matters";

    public const string AddOnPrice = "$25.99/month";

    public const string BuyMailTo =
        "mailto:zerocoolok1983@gmail.com?subject=Legal%20Matters%20add-on";

    /// <summary>
    /// Activates a Legal Matters key the user typed. The key is verified
    /// offline (HMAC signature, product id, expiry, email binding) and stored
    /// on this PC when valid.
    /// </summary>
    public static bool TryActivate(string? keyText, string? email, out string error) =>
        ClaimPilot.Licensing.AppLicense.LegalMatters.TryActivate(keyText, email, out error);
}
