using YachtPilot.Models;
using Windows.Storage;

namespace YachtPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("yacht-seeded.flag") is not null) return;

        var vesselStore = new JsonListStore<Vessel>("vessels.json");
        if ((await vesselStore.LoadAsync()).Count > 0)
        {
            await folder.CreateFileAsync("yacht-seeded.flag", CreationCollisionOption.ReplaceExisting);
            return;
        }

        var v1 = new Vessel
        {
            Name = "M/Y Sample Azure (SAMPLE)", Flag = "Sample Flag", Berth = "Sample Marina, Berth 12",
            OwnerName = "Sample Owner One", OwnerContact = "owner1@sample.invalid",
            Dimensions = "24m motor yacht (sample)", CurrentEngineHours = 4120m,
            InsuranceExpiry = DateTimeOffset.Now.AddMonths(4),
            RegistrationExpiry = DateTimeOffset.Now.AddMonths(9),
        };
        var v2 = new Vessel
        {
            Name = "S/Y Sample Wind (SAMPLE)", Flag = "Sample Flag", Berth = "Sample Marina, Berth 30",
            OwnerName = "Sample Owner Two", OwnerContact = "owner2@sample.invalid",
            Dimensions = "15m sailing yacht (sample)", CurrentEngineHours = 1890m,
            InsuranceExpiry = DateTimeOffset.Now.AddMonths(7),
            RegistrationExpiry = DateTimeOffset.Now.AddMonths(2),
        };
        await vesselStore.SaveAsync(new List<Vessel> { v1, v2 });

        var c1 = new CrewMember { Name = "Sample Captain - Alex R.", Role = "Captain", VesselId = v1.Id, VesselLabel = v1.Name, Phone = "(555) 010-0701" };
        var c2 = new CrewMember { Name = "Sample Mate - Jordan P.", Role = "Mate", VesselId = v1.Id, VesselLabel = v1.Name, Phone = "(555) 010-0702" };
        var c3 = new CrewMember { Name = "Sample Skipper - Sam W.", Role = "Skipper", VesselId = v2.Id, VesselLabel = v2.Name, Phone = "(555) 010-0703" };
        var c4 = new CrewMember { Name = "Sample Crew - Riley D.", Role = "Deckhand", VesselId = v2.Id, VesselLabel = v2.Name, Phone = "(555) 010-0704" };
        await new JsonListStore<CrewMember>("crew.json").SaveAsync(new List<CrewMember> { c1, c2, c3, c4 });

        await new JsonListStore<Certificate>("certificates.json").SaveAsync(new List<Certificate>
        {
            new() { CrewId = c1.Id, CrewLabel = c1.Name, Type = "Master Licence", ExpiryDate = DateTimeOffset.Now.AddMonths(14) },
            // The gate-closer: an expired medical on the captain of vessel 1.
            new() { CrewId = c1.Id, CrewLabel = c1.Name, Type = "ENG1 Medical", ExpiryDate = DateTimeOffset.Now.AddDays(-6), Note = "SAMPLE: expired - this is what closes the charter-ready gate." },
            new() { CrewId = c2.Id, CrewLabel = c2.Name, Type = "STCW Basic Safety", ExpiryDate = DateTimeOffset.Now.AddDays(18), Note = "SAMPLE: renewal course to book - orange band." },
            new() { CrewId = c3.Id, CrewLabel = c3.Name, Type = "Yachtmaster", ExpiryDate = DateTimeOffset.Now.AddMonths(20) },
            new() { CrewId = c4.Id, CrewLabel = c4.Name, Type = "STCW Basic Safety", ExpiryDate = DateTimeOffset.Now.AddMonths(11) },
        });

        await new JsonListStore<MaintenanceItem>("maintenance.json").SaveAsync(new List<MaintenanceItem>
        {
            new() { VesselId = v1.Id, VesselLabel = v1.Name, Title = "Main engine service (sample)", Basis = "Engine hours", LastDoneHours = 3950m, NextDueHours = 4200m, LastDoneDate = DateTimeOffset.Now.AddMonths(-4) },
            new() { VesselId = v1.Id, VesselLabel = v1.Name, Title = "Liferaft service (sample)", Basis = "Date", LastDoneDate = DateTimeOffset.Now.AddMonths(-13), NextDueDate = DateTimeOffset.Now.AddDays(-9), Note = "SAMPLE: overdue - also blocks readiness." },
            new() { VesselId = v2.Id, VesselLabel = v2.Name, Title = "Haul-out & antifouling (sample)", Basis = "Date", LastDoneDate = DateTimeOffset.Now.AddMonths(-10), NextDueDate = DateTimeOffset.Now.AddDays(45) },
            new() { VesselId = v2.Id, VesselLabel = v2.Name, Title = "Rig inspection (sample)", Basis = "Date", LastDoneDate = DateTimeOffset.Now.AddMonths(-11), NextDueDate = DateTimeOffset.Now.AddDays(20) },
        });

        var today = DateTimeOffset.Now.Date;
        await new JsonListStore<Booking>("bookings.json").SaveAsync(new List<Booking>
        {
            new()
            {
                VesselId = v2.Id, VesselLabel = v2.Name, Kind = "Charter",
                ChartererName = "Sample Charterer (fictional)", Broker = "Sample Brokerage",
                StartDate = new DateTimeOffset(today.AddDays(12), TimeSpan.Zero),
                EndDate = new DateTimeOffset(today.AddDays(19), TimeSpan.Zero),
                Rate = 18500m, Currency = "EUR", ApaAdvance = 5550m, ApaSpent = 0m, Status = "Booked",
                Notes = "Sample charter - vessel 2 is charter-ready, so the calendar is clean.",
            },
            new()
            {
                VesselId = v1.Id, VesselLabel = v1.Name, Kind = "Yard",
                StartDate = new DateTimeOffset(today.AddDays(3), TimeSpan.Zero),
                EndDate = new DateTimeOffset(today.AddDays(5), TimeSpan.Zero),
                Status = "Booked", Notes = "Sample yard block - liferaft service window.",
            },
        });

        var month = DateTimeOffset.Now;
        await new JsonListStore<Expense>("expenses.json").SaveAsync(new List<Expense>
        {
            new() { VesselId = v1.Id, VesselLabel = v1.Name, Date = month.AddDays(-4), Category = "Berth", Amount = 1450m, Currency = "USD", Note = "Monthly berth (sample)" },
            new() { VesselId = v1.Id, VesselLabel = v1.Name, Date = month.AddDays(-2), Category = "Maintenance", Amount = 620m, Currency = "USD", Note = "Zincs + diver (sample)" },
            new() { VesselId = v2.Id, VesselLabel = v2.Name, Date = month.AddDays(-6), Category = "Fuel", Amount = 310m, Currency = "EUR", Note = "Diesel top-up (sample)" },
            new() { VesselId = v2.Id, VesselLabel = v2.Name, Date = month.AddDays(-1), Category = "Management fee", Amount = 750m, Currency = "EUR", Note = "Monthly management fee (sample)" },
        });

        await new JsonListStore<Vendor>("vendors.json").SaveAsync(new List<Vendor>
        {
            new() { Name = "Sample Marine Engineering", Trade = "Engineer", Contact = "(555) 010-0710 (sample)", CoiExpiry = DateTimeOffset.Now.AddMonths(3), LastJobNote = "Generator service on M/Y Sample Azure (sample)" },
            new() { Name = "Sample Rigging Co", Trade = "Rigger", Contact = "(555) 010-0711 (sample)", CoiExpiry = DateTimeOffset.Now.AddDays(22), LastJobNote = "Standing rigging check (sample)" },
        });

        await new JsonListStore<ActivityEntry>("activity.json").SaveAsync(new List<ActivityEntry>
        {
            new() { VesselId = v2.Id, VesselLabel = v2.Name, Kind = "Statement sent", Detail = "Prior-month owner statement (sample)", MethodNote = "Email (sample)", Date = DateTimeOffset.Now.AddDays(-9) },
        });

        await folder.CreateFileAsync("yacht-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
