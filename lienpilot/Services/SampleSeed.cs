using LienPilot.Models;
using Windows.Storage;

namespace LienPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("lien-seeded.flag") is not null) return;

        var profileStore = new JsonListStore<StateProfile>("profiles.json");
        var unitStore = new JsonListStore<StorageUnit>("units.json");
        if ((await profileStore.LoadAsync()).Count == 0)
        {
            var sample = new StateProfile
            {
                Name = "SAMPLE profile - verify against your statute and counsel before use",
                IsSample = true,
                NoticeDayOffset = 5,
                WaitingDaysAfterNotice = 30,
                PublicationRequired = true,
                SaleDaysAfterFirstPublication = 15,
                RequiredSteps = new List<string> { "Default notice", "Lien notice", "Advertisement week 1", "Advertisement week 2" },
                Confirmed = false,
                Notes = "SAMPLE ONLY. These numbers are placeholders shaped like a common state pattern. Replace them with your own statute's steps and confirm.",
            };
            await profileStore.SaveAsync(new List<StateProfile> { sample });

            var settingsStore = new FacilitySettingsStore();
            var settings = await settingsStore.LoadAsync();
            settings.ActiveProfileId = sample.Id;
            await settingsStore.SaveAsync(settings);

            var current = new StorageUnit
            {
                UnitNumber = "A-01 (SAMPLE)",
                TenantName = "Sample Tenant Current",
                MonthlyRate = 85m,
                Balance = 0m,
                PaidThrough = DateTimeOffset.Now.AddDays(12),
                Status = "Current",
            };
            var late = new StorageUnit
            {
                UnitNumber = "A-02 (SAMPLE)",
                TenantName = "Sample Tenant Late",
                MonthlyRate = 95m,
                Balance = 115m,
                PaidThrough = DateTimeOffset.Now.AddDays(-9),
                Status = "Late",
                DefaultDate = DateTimeOffset.Now.AddDays(-9),
            };
            var midLien = new StorageUnit
            {
                UnitNumber = "B-07 (SAMPLE)",
                TenantName = "Sample Tenant Mid-Lien",
                MonthlyRate = 110m,
                Balance = 402.50m,
                PaidThrough = DateTimeOffset.Now.AddDays(-52),
                Status = "Advertised",
                DefaultDate = DateTimeOffset.Now.AddDays(-52),
            };
            await unitStore.SaveAsync(new List<StorageUnit> { current, late, midLien });

            var ledgerStore = new JsonListStore<LienLedgerEntry>("ledger.json");
            await ledgerStore.SaveAsync(new List<LienLedgerEntry>
            {
                new()
                {
                    UnitId = midLien.Id, UnitLabel = midLien.DisplayLine, StepType = "Default notice",
                    Date = DateTimeOffset.Now.AddDays(-47), Method = "Verified mail",
                    Tracking = "SAMPLE-TRACKING", Note = "Sample entry - the Advertisement week 2 step is deliberately missing so you can see the sale-ready block work.",
                },
                new()
                {
                    UnitId = midLien.Id, UnitLabel = midLien.DisplayLine, StepType = "Lien notice",
                    Date = DateTimeOffset.Now.AddDays(-40), Method = "Verified mail",
                    Tracking = "SAMPLE-TRACKING", Note = "Sample entry.",
                },
                new()
                {
                    UnitId = midLien.Id, UnitLabel = midLien.DisplayLine, StepType = "Advertisement week 1",
                    Date = DateTimeOffset.Now.AddDays(-6), Method = "Publication",
                    Tracking = "", Note = "Sample entry.",
                },
            });
        }
        await folder.CreateFileAsync("lien-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
