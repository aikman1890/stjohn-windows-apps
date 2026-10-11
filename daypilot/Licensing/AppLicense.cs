using StJohn.Licensing;

namespace DayPilot.Licensing;

/// <summary>DayPilot's licensing wiring: $2.99/month with a 4-day free trial.</summary>
public static class AppLicense
{
    public static LicenseConfig Config { get; } = new()
    {
        AppName = "DayPilot",
        Product = ProductId.DayPilot,
        TrialDays = 4,
        PriceDisplay = "$2.99/month",
        BuyMailto = "mailto:zerocoolok1983@gmail.com?subject=DayPilot%20purchase",
        Secret = () => LicenseSecrets.DayPilot,
    };

    public static LicenseManager Manager { get; } = new LicenseManager(Config);
}
