using StJohn.Licensing;

namespace Wuse.Licensing;

/// <summary>
/// wuse's licensing wiring:
///   Personal $4.99/month gates the app itself (no free trial).
///   Business $9.99/month gates the app too, plus: up to 5 Microsoft work
///   accounts (Office 365), the password vault, and priority voice.
/// A Business key unlocks everything a Personal key does.
/// </summary>
public static class AppLicense
{
    public static LicenseConfig Config { get; } = new()
    {
        AppName = "wuse",
        Product = ProductId.Wuse,
        TrialDays = 0,
        PriceDisplay = "$4.99/month",
        BuyMailto = "mailto:zerocoolok1983@gmail.com?subject=wuse%20purchase",
        Secret = () => LicenseSecrets.Wuse,
    };

    public static LicenseConfig BusinessConfig { get; } = new()
    {
        AppName = "wuse",
        Product = ProductId.WuseBusiness,
        TrialDays = 0,
        PriceDisplay = "$9.99/month",
        BuyMailto = "mailto:zerocoolok1983@gmail.com?subject=wuse%20Business%20purchase",
        Secret = () => LicenseSecrets.WuseBusiness,
    };

    public static LicenseManager Manager { get; } = new LicenseManager(Config);
    public static LicenseManager Business { get; } = new LicenseManager(BusinessConfig);

    /// <summary>True when a valid Business key is stored.</summary>
    public static bool IsBusinessLicensed => Business.HasValidKey();

    /// <summary>True when either tier unlocks the app (Business includes Personal).</summary>
    public static bool HasAppAccess => Manager.HasAccess() || Business.HasAccess();

    /// <summary>Display tier name for settings/status surfaces.</summary>
    public static string TierName => IsBusinessLicensed ? "Business" : "Personal";

    /// <summary>
    /// Activates whichever tier the key belongs to. Tries Business first
    /// (distinct product byte), then Personal. Personal-key behavior is
    /// unchanged.
    /// </summary>
    public static bool TryActivateAny(string? keyText, string? email, out string error)
    {
        if (Business.TryActivate(keyText, email, out _))
        {
            error = string.Empty;
            return true;
        }
        return Manager.TryActivate(keyText, email, out error);
    }

    /// <summary>Status line covering both tiers (Business shown first).</summary>
    public static string GetStatusText() =>
        IsBusinessLicensed ? Business.GetStatusText() : Manager.GetStatusText();

    // ------------------------------------------------------------------
    // Feature gating
    // ------------------------------------------------------------------

    /// <summary>Max Microsoft WORK accounts (Office 365): 5 on Business, 0 on Personal.</summary>
    public static int MaxWorkAccounts => IsBusinessLicensed ? 5 : 0;

    /// <summary>Personal tier always gets 1 personal Microsoft account (Hotmail/Outlook).</summary>
    public const int MaxPersonalMicrosoftAccounts = 1;

    /// <summary>True when another work account may be connected.</summary>
    public static bool CanConnectWorkAccount(int currentWorkAccounts) =>
        currentWorkAccounts < MaxWorkAccounts;

    /// <summary>Warm, never-pushy upsell shown when a Personal user hits the work-account gate.</summary>
    public const string WorkAccountUpsell =
        "Connecting extra work accounts is part of wuse Business ($9.99/month) — " +
        "it also unlocks the password vault and priority voice. " +
        "Your personal account works just the same here. Want me to tell you how to upgrade?";

    /// <summary>The password vault is a Business feature.</summary>
    public static bool CanUseVault => IsBusinessLicensed;

    /// <summary>Warm, never-pushy upsell for the vault gate.</summary>
    public const string VaultUpsell =
        "The password vault comes with wuse Business ($9.99/month), along with up to " +
        "5 work accounts and priority voice. Happy to tell you how to upgrade whenever you like.";
}
