using HuntPilot.Models;

namespace HuntPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var settingsStore = new HuntSettingsStore();
        var settings = await settingsStore.LoadAsync();
        if (settings.SampleSeeded) return;

        var now = DateTimeOffset.Now;

        settings.RanchName = "Sample Mesquite Ranch (SAMPLE)";
        settings.SampleSeeded = true;
        await settingsStore.SaveAsync(settings);

        await new JsonListStore<SpeciesEntry>("species.json").SaveAsync(new List<SpeciesEntry>
        {
            new() { Name = "Blackbuck (SAMPLE price)", TrophyFee = 5500m, SeasonNote = "Year-round as recorded by the ranch", MethodsNote = "Rifle / bow as the ranch allows", HerdNote = "Strong herd, sample note" },
            new() { Name = "Addax (SAMPLE price)", TrophyFee = 12000m, SeasonNote = "Year-round as recorded by the ranch", HerdNote = "Small herd - book ahead" },
            new() { Name = "Nile Lechwe (SAMPLE price)", TrophyFee = 17500m, SeasonNote = "Seasonal as recorded by the ranch", MethodsNote = "Spot and stalk", HerdNote = "Premium species, sample note" },
        });

        var h1 = new Hunter { Name = "Sample Wade Callahan", Contact = "sample hunter - no real contact", LicenseNumber = "SAMPLE-TX-001", LicenseState = "TX", LicenseExpiry = now.AddDays(200), PartyNote = "Hunts with his son", Preferences = "Wishlist: Nile Lechwe - lodge room 2 - no special dietary notes" };
        var h2 = new Hunter { Name = "Sample Priya Nair", Contact = "sample hunter - no real contact", LicenseNumber = "SAMPLE-TX-002", LicenseState = "TX", LicenseExpiry = now.AddDays(25), Preferences = "Wishlist: Addax - license renewal falls inside 30 days, watch the file" };
        var h3 = new Hunter { Name = "Sample Gus Whitfield", Contact = "sample hunter - no real contact", LicenseNumber = "SAMPLE-OK-003", LicenseState = "OK", LicenseExpiry = now.AddDays(400), Preferences = "Repeat client - 3 seasons" };
        await new JsonListStore<Hunter>("hunters.json").SaveAsync(new List<Hunter> { h1, h2, h3 });

        var hunt1 = new HuntBooking
        {
            HunterId = h3.Id, HunterLabel = h3.Name, StartDate = now.AddDays(-80), EndDate = now.AddDays(-78),
            GuideLabel = "Sample Guide R. Flores", SpeciesTargeted = "Blackbuck", PackageLabel = "All-inclusive hunt package (SAMPLE)",
            PackagePrice = 1500m, DepositReceived = 750m, TrophyFees = 5500m, PaymentsReceived = 6250m,
            LodgingNights = 2, NonHunterGuests = 1, NonHunterRatePerNight = 500m,
            WaiverStatus = "Signed", WaiverVersion = "v2026.1", Status = "Completed",
            ArrivalNote = "Sample arrival note", RoomNote = "Lodge room 1",
        };
        var hunt2 = new HuntBooking
        {
            HunterId = h1.Id, HunterLabel = h1.Name, StartDate = now.AddDays(12), EndDate = now.AddDays(14),
            GuideLabel = "Sample Guide R. Flores", SpeciesTargeted = "Nile Lechwe", PackageLabel = "All-inclusive hunt package (SAMPLE)",
            PackagePrice = 1500m, DepositReceived = 750m, TrophyFees = 0m, PaymentsReceived = 0m,
            LodgingNights = 2, NonHunterGuests = 0, NonHunterRatePerNight = 500m,
            WaiverStatus = "Partial", WaiverVersion = "v2026.1", Status = "Booked",
            Notes = "Balance clock: the trophy fee lands at harvest - the follow-up list shows the deposit still to be matched by the balance.",
        };
        await new JsonListStore<HuntBooking>("hunts.json").SaveAsync(new List<HuntBooking> { hunt1, hunt2 });

        var harv1 = new Harvest { HuntId = hunt1.Id, HunterLabel = h3.Name, SpeciesLabel = "Blackbuck", Date = now.AddDays(-79), GuideLabel = "Sample Guide R. Flores", Measurements = "Horns 21 in (sample)", FieldDressed = true, FieldDressedDate = now.AddDays(-79), TrophyFee = 5500m };
        var harv2 = new Harvest { HuntId = hunt1.Id, HunterLabel = h3.Name, SpeciesLabel = "Feral hog (management)", Date = now.AddDays(-78), GuideLabel = "Sample Guide R. Flores", FieldDressed = true, FieldDressedDate = now.AddDays(-78), TrophyFee = 0m, Notes = "Meat only - no taxidermy." };
        await new JsonListStore<Harvest>("harvests.json").SaveAsync(new List<Harvest> { harv1, harv2 });

        await new JsonListStore<TrophyCase>("trophies.json").SaveAsync(new List<TrophyCase>
        {
            new() { HarvestId = harv1.Id, HunterLabel = h3.Name, SpeciesLabel = "Blackbuck", Stage = "At taxidermist", StageDate = now.AddDays(-75), ProcessorName = "Sample Meat Processor", ProcessorTicket = "MP-1042 (sample)", TaxidermistName = "Sample Taxidermy Studio", WorkOrder = "WO-2210 (sample)", Notes = "75 days in stage against a 30-day norm - the chain board shows this one RED. This is the trophy that gets forgotten without a clock." },
            new() { HarvestId = harv2.Id, HunterLabel = h3.Name, SpeciesLabel = "Feral hog (meat)", Stage = "Meat shipped/delivered", StageDate = now.AddDays(-70), ProcessorName = "Sample Meat Processor", ProcessorTicket = "MP-1043 (sample)", ShipNote = "Delivered to the client (sample note)" },
            new() { HarvestId = Guid.NewGuid(), HunterLabel = h2.Name, SpeciesLabel = "Addax (prior season, sample)", Stage = "Mount complete", StageDate = now.AddDays(-6), TaxidermistName = "Sample Taxidermy Studio", WorkOrder = "WO-1980 (sample)", Notes = "Mount complete - crating and shipping are the next calls on the follow-up list." },
        });
    }
}
