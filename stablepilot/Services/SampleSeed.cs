using StablePilot.Models;
using Windows.Storage;

namespace StablePilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("stable-seeded.flag") is not null) return;

        var horseStore = new JsonListStore<Horse>("horses.json");
        if ((await horseStore.LoadAsync()).Count > 0)
        {
            await folder.CreateFileAsync("stable-seeded.flag", CreationCollisionOption.ReplaceExisting);
            return;
        }

        // FICTIONAL horses and owners.
        var o1 = new HorseOwner { Name = "Sample Owner - C. Beaumont (fictional)", Contact = "owner@sample.invalid", Phone = "(555) 010-1001" };
        var o2 = new HorseOwner { Name = "Sample Owner - D. Reyes (fictional)", Contact = "owner2@sample.invalid", Phone = "(555) 010-1002" };
        await new JsonListStore<HorseOwner>("owners.json").SaveAsync(new List<HorseOwner> { o1, o2 });

        var h1 = new Horse
        {
            Name = "Sample Horse - Aurelia (fictional)", OwnerId = o1.Id, OwnerLabel = o1.Name, Stall = "4",
            ArrivalDate = DateTimeOffset.Now.AddMonths(-8), BreedAgeNote = "Warmblood mare, 9 (sample)",
            FeedAm = "2 flakes hay + 2 qt senior feed (sample)", FeedPm = "2 flakes hay + 2 qt senior feed (sample)",
            Supplements = "Joint supplement AM (sample)", TurnoutGroup = "Mares' field (sample)",
            BlanketRule = "Medium blanket below 40F (sample)", VetLabel = "Sample Equine Vet", FarrierLabel = "Sample Farrier",
            BoardRateMonthly = 1450m, Currency = "USD",
        };
        var h2 = new Horse
        {
            Name = "Sample Horse - Bramble (fictional)", OwnerId = o1.Id, OwnerLabel = o1.Name, Stall = "5",
            ArrivalDate = DateTimeOffset.Now.AddMonths(-5), BreedAgeNote = "Thoroughbred gelding, 12 (sample)",
            FeedAm = "2 flakes hay + 3 qt performance feed (sample)", FeedPm = "2 flakes hay + 3 qt performance feed (sample)",
            TurnoutGroup = "Geldings' field (sample)", BlanketRule = "Light sheet below 45F (sample)",
            VetLabel = "Sample Equine Vet", FarrierLabel = "Sample Farrier",
            BoardRateMonthly = 1450m, Currency = "USD",
        };
        var h3 = new Horse
        {
            Name = "Sample Horse - Calloway (fictional)", OwnerId = o2.Id, OwnerLabel = o2.Name, Stall = "7",
            ArrivalDate = DateTimeOffset.Now.AddMonths(-14), BreedAgeNote = "Dutch Warmblood gelding, 8 (sample)",
            FeedAm = "3 flakes hay + ration balancer (sample)", FeedPm = "3 flakes hay + ration balancer (sample)",
            TurnoutGroup = "Solo paddock (sample)", BlanketRule = "Heavy below 30F (sample)",
            VetLabel = "Sample Equine Vet", FarrierLabel = "Sample Farrier",
            BoardRateMonthly = 1600m, Currency = "USD",
        };
        var h4 = new Horse
        {
            Name = "Sample Horse - Duchess (fictional)", OwnerId = o2.Id, OwnerLabel = o2.Name, Stall = "9",
            ArrivalDate = DateTimeOffset.Now.AddMonths(-3), BreedAgeNote = "Hanoverian mare, 11 (sample)",
            FeedAm = "2 flakes hay + 2 qt senior feed (sample)", FeedPm = "2 flakes hay + 2 qt senior feed (sample)",
            Supplements = "Ulcer support daily (sample)", TurnoutGroup = "Mares' field (sample)",
            BlanketRule = "Medium blanket below 40F (sample)", VetLabel = "Sample Equine Vet", FarrierLabel = "Sample Farrier",
            BoardRateMonthly = 1450m, Currency = "USD",
        };
        await horseStore.SaveAsync(new List<Horse> { h1, h2, h3, h4 });

        var today = DateTimeOffset.Now;
        await new JsonListStore<CareClock>("clocks.json").SaveAsync(new List<CareClock>
        {
            // Coggins inside its 30-day orange band.
            new() { HorseId = h1.Id, HorseLabel = h1.Name, Kind = "Coggins", Detail = "EIA test (sample)", LastDoneDate = today.AddDays(-345), NextDueDate = today.AddDays(20), CycleDays = 365 },
            new() { HorseId = h1.Id, HorseLabel = h1.Name, Kind = "Farrier", LastDoneDate = today.AddDays(-40), NextDueDate = today.AddDays(5), CycleDays = 45 },
            // Farrier overdue.
            new() { HorseId = h2.Id, HorseLabel = h2.Name, Kind = "Farrier", LastDoneDate = today.AddDays(-52), NextDueDate = today.AddDays(-7), CycleDays = 45 },
            new() { HorseId = h2.Id, HorseLabel = h2.Name, Kind = "Coggins", Detail = "EIA test (sample)", LastDoneDate = today.AddDays(-200), NextDueDate = today.AddDays(165), CycleDays = 365 },
            new() { HorseId = h3.Id, HorseLabel = h3.Name, Kind = "Vaccination", Detail = "Spring series (sample)", LastDoneDate = today.AddMonths(-11), NextDueDate = today.AddDays(10), CycleDays = 365 },
            new() { HorseId = h3.Id, HorseLabel = h3.Name, Kind = "Dental", LastDoneDate = today.AddMonths(-10), NextDueDate = today.AddMonths(2), CycleDays = 365 },
            new() { HorseId = h4.Id, HorseLabel = h4.Name, Kind = "Deworming", Detail = "Rotation (sample)", LastDoneDate = today.AddDays(-50), NextDueDate = today.AddDays(10), CycleDays = 60 },
        });

        await new JsonListStore<HealthEntry>("health.json").SaveAsync(new List<HealthEntry>
        {
            new() { HorseId = h2.Id, HorseLabel = h2.Name, Date = today.AddDays(-3), Kind = "Lameness", Detail = "Mild left-front sensitivity noted after schooling; cold-hosed, recheck Friday (sample entry).", Provider = "Barn manager (sample)" },
        });

        await new JsonListStore<SessionEntry>("sessions.json").SaveAsync(new List<SessionEntry>
        {
            new() { HorseId = h3.Id, HorseLabel = h3.Name, Date = today.AddDays(-2), Trainer = "Sample Trainer", Kind = "Training session", Fee = 65m, Note = "Schooling over fences (sample)" },
            new() { HorseId = h3.Id, HorseLabel = h3.Name, Date = today.AddDays(-5), Trainer = "Sample Trainer", Kind = "Training session", Fee = 65m, Note = "Flatwork (sample)" },
            new() { HorseId = h1.Id, HorseLabel = h1.Name, Date = today.AddDays(-4), Trainer = "Sample Trainer", Kind = "Training session", Fee = 65m, Note = "Lunge + ride (sample)" },
        });

        await new JsonListStore<Provider>("providers.json").SaveAsync(new List<Provider>
        {
            new() { Name = "Sample Equine Vet", Trade = "Vet", Contact = "(555) 010-1010 (sample)", HorsesNote = "All barn horses (sample)", LastVisitNote = "Spring vaccinations round (sample)", NextRoundNote = "Coggins draw for Aurelia (sample)" },
            new() { Name = "Sample Farrier", Trade = "Farrier", Contact = "(555) 010-1011 (sample)", HorsesNote = "All barn horses (sample)", NextRoundNote = "Full barn day next Tuesday (sample) - Bramble is overdue" },
        });

        await folder.CreateFileAsync("stable-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
