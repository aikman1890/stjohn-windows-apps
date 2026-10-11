using Windows.Services.Store;

namespace ClaimPilot.Services;

/// <summary>
/// Bankruptcy Plus in-app purchase via the Microsoft Store.
/// Uses the real StoreContext API against the durable add-on product ID from
/// <see cref="Pricing.BankruptcyPlusProductId"/>. Every call is guarded: if
/// the Store is unreachable (sideloaded dev builds, offline, sandbox), the
/// methods return a graceful failure instead of throwing.
/// </summary>
public static class PurchaseService
{
    /// <summary>
    /// True when the signed-in user owns the Bankruptcy Plus add-on — or when
    /// the local Creator Unlock is active (the creator uses everything free on
    /// his own machines; see <see cref="CreatorUnlockService"/>).
    /// </summary>
    public static async Task<bool> IsBankruptcyPlusOwnedAsync()
    {
        // Creator Unlock short-circuits the Store check: local, PIN-gated,
        // documented in BUILD.md. Not a bypass for anyone else.
        if (CreatorUnlockService.IsUnlocked)
        {
            return true;
        }

        try
        {
            var context = StoreContext.GetDefault();
            var result = await context.GetAppLicenseAsync();
            if (result.ExtendedError is not null)
            {
                return false;
            }

            foreach (var addon in result.License.AddOnLicenses.Values)
            {
                if (string.Equals(addon.InAppOfferToken, Pricing.BankruptcyPlusProductId, StringComparison.OrdinalIgnoreCase)
                    && addon.IsActive)
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Runs the Store purchase flow for Bankruptcy Plus.</summary>
    public static async Task<(bool Success, string Message)> PurchaseBankruptcyPlusAsync()
    {
        try
        {
            var context = StoreContext.GetDefault();
            var result = await context.RequestPurchaseAsync(Pricing.BankruptcyPlusProductId);
            if (result.ExtendedError is not null)
            {
                return (false, "The Store could not complete the purchase right now. Try again later.");
            }

            return result.Status switch
            {
                StorePurchaseStatus.Succeeded => (true, "Bankruptcy Plus unlocked. The bankruptcy modules are now available."),
                StorePurchaseStatus.AlreadyPurchased => (true, "Bankruptcy Plus was already purchased. The modules are now available."),
                StorePurchaseStatus.NotPurchased => (false, "The purchase was not completed. No charge was made."),
                StorePurchaseStatus.NetworkError => (false, "Network error — check your connection and try again."),
                StorePurchaseStatus.ServerError => (false, "The Store had a server error. Try again later."),
                _ => (false, "The purchase could not be completed. No charge was made.")
            };
        }
        catch (Exception ex)
        {
            return (false, "Store purchase is unavailable right now: " + ex.Message);
        }
    }
}
