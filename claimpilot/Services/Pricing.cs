namespace ClaimPilot.Services;

/// <summary>
/// All app prices in one place. Base app price is set in the Microsoft Store
/// listing; these constants mirror it for in-app display. Never hardcode a
/// price anywhere else — reference these constants.
/// </summary>
public static class Pricing
{
    /// <summary>Base ClaimPilot app price (monthly subscription, set in the Store listing).</summary>
    public const decimal BasePrice = 14.99m;

    public const string BasePriceDisplay = "$14.99/month";

    /// <summary>Bankruptcy Plus add-on: monthly subscription unlocking the bankruptcy modules and course tracker.</summary>
    public const decimal BankruptcyPlusPrice = 9.99m;

    public const string BankruptcyPlusPriceDisplay = "$9.99/month";

    /// <summary>Legacy Store product ID — unused in the direct-sale build (kept for reference).</summary>
    public const string BankruptcyPlusProductId = "ClaimPilot.BankruptcyPlus";

    public const string BankruptcyPlusName = "Bankruptcy Plus";

    public const string BankruptcyPlusBlurb =
        "Unlocks the Chapter 7 and Chapter 13 guides, document checklists, bankruptcy deadline presets, " +
        "and the required-courses tracker with certificate vault. $9.99/month subscription — cancel anytime.";
}
