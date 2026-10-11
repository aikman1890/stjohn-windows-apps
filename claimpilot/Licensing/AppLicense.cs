using StJohn.Licensing;

namespace ClaimPilot.Licensing;

/// <summary>
/// ClaimPilot's licensing wiring:
///   Base $14.99/month gates the app itself (no free trial).
///   Bankruptcy Plus $9.99/month gates the bankruptcy modules.
///   Legal Matters $25.99/month gates the legal-matter builders.
/// The 7-tap creator unlock (CreatorUnlockService) unlocks everything and is
/// checked before any of these managers.
/// </summary>
public static class AppLicense
{
    public static LicenseConfig BaseConfig { get; } = new()
    {
        AppName = "ClaimPilot",
        Product = ProductId.ClaimPilot,
        TrialDays = 0,
        PriceDisplay = "$14.99/month",
        BuyMailto = "mailto:zerocoolok1983@gmail.com?subject=ClaimPilot%20purchase",
        Secret = () => LicenseSecrets.ClaimPilot,
    };

    public static LicenseConfig PlusConfig { get; } = new()
    {
        AppName = "ClaimPilot",
        Product = ProductId.ClaimPilotPlus,
        TrialDays = 0,
        PriceDisplay = "$9.99/month",
        BuyMailto = "mailto:zerocoolok1983@gmail.com?subject=ClaimPilot%20Bankruptcy%20Plus",
        Secret = () => LicenseSecrets.ClaimPilot,
    };

    public static LicenseConfig LegalMattersConfig { get; } = new()
    {
        AppName = "ClaimPilot",
        Product = ProductId.LegalMatters,
        TrialDays = 0,
        PriceDisplay = "$25.99/month",
        BuyMailto = "mailto:zerocoolok1983@gmail.com?subject=Legal%20Matters%20add-on",
        Secret = () => LicenseSecrets.ClaimPilot,
    };

    public static LicenseManager Base { get; } = new LicenseManager(BaseConfig);
    public static LicenseManager Plus { get; } = new LicenseManager(PlusConfig);
    public static LicenseManager LegalMatters { get; } = new LicenseManager(LegalMattersConfig);

    /// <summary>True when the creator unlock is active (unlocks everything).</summary>
    public static bool CreatorUnlocked => Services.CreatorUnlockService.IsUnlocked;
}
