using DenialPilot.Models;
using Windows.Storage;

namespace DenialPilot.Services;

/// <summary>
/// Seeds one clearly-labeled fictional sample case on first run only, so a
/// new user sees the pipeline, clocks, and ledger working before entering
/// anything real. The seed flag file prevents re-seeding after deletion.
/// </summary>
public static class SampleSeed
{
    public static async Task EnsureAsync(JsonListStore<DenialCase> store)
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            if (await folder.TryGetItemAsync("denialpilot-seeded.flag") is not null) return;

            var items = await store.LoadAsync();
            if (items.Count == 0)
            {
                items.Add(new DenialCase
                {
                    PatientName = "Sample Patient",
                    Insurer = "Sample Insurance Co. (SAMPLE)",
                    Plan = "Sample Plan",
                    ClaimNumber = "SAMPLE-0001",
                    ServiceDate = DateTimeOffset.Now.AddDays(-45),
                    Provider = "Sample Provider",
                    AmountDenied = "$1,250.00",
                    DenialDate = DateTimeOffset.Now.AddDays(-30),
                    ReasonCategory = "Administrative",
                    Status = "Level 1 appeal",
                    Notes = "SAMPLE case - fictional, here so you can see how DenialPilot works. Delete it any time.",
                });
                await store.SaveAsync(items);
            }
            await folder.CreateFileAsync("denialpilot-seeded.flag", CreationCollisionOption.ReplaceExisting);
        }
        catch (Exception)
        {
            // Seeding must never crash the app.
        }
    }
}
