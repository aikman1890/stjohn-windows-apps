using FarewellPilot.Models;
using Windows.Storage;

namespace FarewellPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("farewell-seeded.flag") is not null) return;

        var settingsStore = new FarewellSettingsStore();
        var settings = await settingsStore.LoadAsync();
        var caseStore = new JsonListStore<FuneralCase>("cases.json");
        if ((await caseStore.LoadAsync()).Count == 0)
        {
            var sample = new FuneralCase
            {
                CaseNumber = "SAMPLE-001",
                DecedentName = "Sample Decedent",
                DateOfDeath = DateTimeOffset.Now.AddDays(-3),
                NextOfKin = "Sample Next of Kin",
                KinRelationship = "Daughter",
                ServiceType = "Burial",
                Status = "Paperwork pending",
                FirstCallDate = DateTimeOffset.Now.AddDays(-3),
                ServiceDate = DateTimeOffset.Now.AddDays(2),
                ServiceLocation = "Sample Chapel",
                Checklist = CaseMath.BuildChecklist(settings, "Burial"),
                Notes = "SAMPLE case - delete from the Cases page. The death-certificate clock is running so you can see the panel work.",
            };
            sample.Checklist[0].Done = true;
            sample.Checklist[0].ConfirmedBy = "Sample Director";
            sample.Checklist[0].ConfirmedDate = DateTimeOffset.Now.AddDays(-2);
            await caseStore.SaveAsync(new List<FuneralCase> { sample });
            await new JsonListStore<ContractLine>("contractlines.json").SaveAsync(new List<ContractLine>
            {
                new() { CaseId = sample.Id, Category = "Professional services", Description = "Basic services of funeral director and staff (sample)", Amount = 2195m },
                new() { CaseId = sample.Id, Category = "Merchandise", Description = "Casket (sample)", Amount = 2495m },
                new() { CaseId = sample.Id, Category = "Cash advance", Description = "Cemetery charges advanced (sample)", Amount = 850m },
            });
            await new JsonListStore<Payment>("payments.json").SaveAsync(new List<Payment>
            {
                new() { CaseId = sample.Id, Date = DateTimeOffset.Now.AddDays(-2), Amount = 2000m, Method = "Check", Note = "Sample payment" },
            });

            var pre = new PreneedContract
            {
                Number = "PN-SAMPLE-001",
                Purchaser = "Sample Purchaser",
                Beneficiary = "Sample Beneficiary",
                Date = DateTimeOffset.Now.AddDays(-6),
                TotalAmount = 8500m,
                FundingType = "Trust",
                Status = "Active",
                Notes = "SAMPLE preneed - one deposit logged; the rest is still inside the deposit window so the reconciliation view shows both states.",
            };
            await new JsonListStore<PreneedContract>("preneed.json").SaveAsync(new List<PreneedContract> { pre });
            await new JsonListStore<DepositEntry>("deposits.json").SaveAsync(new List<DepositEntry>
            {
                new() { PreneedId = pre.Id, Date = DateTimeOffset.Now.AddDays(-4), Amount = 4000m, Destination = "Sample Trust Account", Note = "Sample deposit" },
            });
        }
        await folder.CreateFileAsync("farewell-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
