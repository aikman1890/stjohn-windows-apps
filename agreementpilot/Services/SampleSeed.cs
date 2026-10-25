using AgreementPilot.Models;
using Windows.Storage;

namespace AgreementPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("agreement-seeded.flag") is not null) return;

        var customerStore = new JsonListStore<TradeCustomer>("customers.json");
        if ((await customerStore.LoadAsync()).Count == 0)
        {
            var c1 = new TradeCustomer { Name = "Sample Customer One", Address = "101 Sample Lane", Phone = "(555) 010-0101" };
            var c2 = new TradeCustomer { Name = "Sample Customer Two", Address = "202 Sample Lane", Phone = "(555) 010-0102" };
            await customerStore.SaveAsync(new List<TradeCustomer> { c1, c2 });

            var furnace = new Equipment
            {
                CustomerId = c1.Id, CustomerLabel = c1.DisplayLine, Type = "Furnace",
                Brand = "SampleBrand", Model = "SB-80", Serial = "SAMPLE-SN-1",
                InstallDate = DateTimeOffset.Now.AddYears(-19).AddMonths(-3),
                LocationNote = "Attic",
            };
            var ac = new Equipment
            {
                CustomerId = c1.Id, CustomerLabel = c1.DisplayLine, Type = "Air conditioner",
                Brand = "SampleBrand", Model = "SB-AC3", Serial = "SAMPLE-SN-2",
                InstallDate = DateTimeOffset.Now.AddYears(-6),
                LocationNote = "Side yard",
            };
            var heater = new Equipment
            {
                CustomerId = c2.Id, CustomerLabel = c2.DisplayLine, Type = "Water heater",
                Brand = "SampleBrand", Model = "SB-WH50", Serial = "SAMPLE-SN-3",
                InstallDate = DateTimeOffset.Now.AddYears(-11),
                LocationNote = "Garage",
            };
            await new JsonListStore<Equipment>("equipment.json").SaveAsync(new List<Equipment> { furnace, ac, heater });

            var a1 = new Agreement
            {
                CustomerId = c1.Id, CustomerLabel = c1.DisplayLine, PlanName = "Comfort Plan",
                Price = 289m, StartDate = DateTimeOffset.Now.AddDays(-340),
                RenewalDate = DateTimeOffset.Now.AddDays(25),
                VisitsIncluded = 2, VisitsDelivered = 1, Status = "Active",
                CoveredNote = "Furnace + AC (sample)",
            };
            var a2 = new Agreement
            {
                CustomerId = c2.Id, CustomerLabel = c2.DisplayLine, PlanName = "Comfort Plan",
                Price = 249m, StartDate = DateTimeOffset.Now.AddDays(-400),
                RenewalDate = DateTimeOffset.Now.AddDays(-12),
                VisitsIncluded = 2, VisitsDelivered = 2, Status = "Lapsed",
                CoveredNote = "Water heater (sample)",
                Notes = "Sample lapsed agreement - the win-back list on the Dashboard.",
            };
            await new JsonListStore<Agreement>("agreements.json").SaveAsync(new List<Agreement> { a1, a2 });

            await new JsonListStore<Visit>("visits.json").SaveAsync(new List<Visit>
            {
                new()
                {
                    CustomerId = c1.Id, CustomerLabel = c1.DisplayLine,
                    EquipmentId = furnace.Id, EquipmentLabel = furnace.FullLabel,
                    AgreementId = a1.Id, Date = DateTimeOffset.Now.AddMonths(-5),
                    WorkPerformed = "Fall tune-up (sample)",
                    Readings = "Heat exchanger visually OK; CO test passed (sample readings)",
                    Recommendation = "Start planning furnace replacement - 19 years old, efficiency dropping",
                    RecResponse = "Declined", RecAmount = 6800m,
                },
            });
        }
        await folder.CreateFileAsync("agreement-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
