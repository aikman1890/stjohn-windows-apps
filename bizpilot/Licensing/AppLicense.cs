using StJohn.Licensing;

namespace BizPilot.Licensing;

/// <summary>BizPilot's licensing wiring: $9.99/month, no free trial.</summary>
public static class AppLicense
{
    public static LicenseConfig Config { get; } = new()
    {
        AppName = "BizPilot",
        Product = ProductId.BizPilot,
        TrialDays = 0,
        PriceDisplay = "$9.99/month",
        BuyMailto = "mailto:zerocoolok1983@gmail.com?subject=BizPilot%20purchase",
        Secret = () => LicenseSecrets.BizPilot,
    };

    public static LicenseManager Manager { get; } = new LicenseManager(Config);
}
