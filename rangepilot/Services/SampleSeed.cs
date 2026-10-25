using RangePilot.Models;

namespace RangePilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var settingsStore = new RangeSettingsStore();
        var settings = await settingsStore.LoadAsync();
        if (settings.SampleSeeded) return;

        var now = DateTimeOffset.Now;
        var today = now.Date;

        settings.BusinessName = "Sample Canyon Experiences (SAMPLE)";
        settings.SampleSeeded = true;
        await settingsStore.SaveAsync(settings);

        await new JsonListStore<ExperiencePackage>("packages.json").SaveAsync(new List<ExperiencePackage>
        {
            new() { Name = "Tank Drive Experience", Price = 750m, DurationNote = "about 1 hour", Capacity = 6, AssetsRequired = "Sherman tank", IncludesNote = "Drive course + passenger ride" },
            new() { Name = "Range Day Package", Price = 299m, DurationNote = "about 2 hours", Capacity = 8, AssetsRequired = "Lane 2", IncludesNote = "Range time + safety officer" },
            new() { Name = "Fan Favorite (SAMPLE)", Price = 4950m, DurationNote = "half day", Capacity = 6, AssetsRequired = "Sherman tank + Lane 2", IncludesNote = "Drive + range + video package included" },
        });

        var assets = new List<AssetItem>
        {
            new() { Name = "Sherman tank", Kind = "Vehicle", Status = "Up", LastServiceNote = "Full service 12 days ago", NextServiceNote = "Track inspection in 18 days" },
            new() { Name = "Scorpion light tank", Kind = "Vehicle", Status = "Down", LastServiceNote = "Track fault found 2 days ago", NextServiceNote = "Awaiting track parts - DO NOT BOOK", Notes = "Guests booked against this asset need a swap or a call-ahead." },
            new() { Name = "Lane 2", Kind = "Range lane", Status = "Up", NextServiceNote = "Backstop check weekly" },
        };
        await new JsonListStore<AssetItem>("assets.json").SaveAsync(assets);
        await new JsonListStore<AssetLogEntry>("assetlog.json").SaveAsync(new List<AssetLogEntry>
        {
            new() { AssetId = assets[1].Id, AssetLabel = assets[1].Name, Date = now.AddDays(-2), Kind = "Maintenance", Detail = "Track fault found on inspection - vehicle DOWN until parts arrive." },
            new() { AssetId = assets[0].Id, AssetLabel = assets[0].Name, Date = now.AddDays(-1), Kind = "Usage", Detail = "Two drive experiences run; fuel topped up." },
        });

        await new JsonListStore<StaffMember>("staff.json").SaveAsync(new List<StaffMember>
        {
            new() { Name = "Sample Sam Ortiz", Role = "Safety officer", Certifications = "Range safety officer", CertExpiry = now.AddDays(160), ScheduleNote = "Weekends" },
            new() { Name = "Sample Dana Ruiz", Role = "Driver / instructor", Certifications = "Tracked-vehicle instructor", CertExpiry = now.AddDays(20), ScheduleNote = "Cert renewal due inside 30 days - watch the roster." },
            new() { Name = "Sample Lee Park", Role = "Guide", Certifications = "First aid", CertExpiry = now.AddDays(300) },
        });

        var b1 = new Booking { PackageLabel = "Tank Drive Experience", LeadGuest = "Sample Carter Party", PartySize = 4, SlotDate = new DateTimeOffset(today, now.Offset), SlotTimeNote = "10:00", Source = "Direct", TotalPrice = 750m, DepositPaid = 750m, AssetLabel = "Sherman tank", StaffNote = "Ortiz (safety) + Ruiz (driver)", Status = "Booked" };
        var b2 = new Booking { PackageLabel = "Range Day Package", LeadGuest = "Sample Nguyen Party", PartySize = 6, SlotDate = new DateTimeOffset(today, now.Offset), SlotTimeNote = "13:00", Source = "Groupon", TotalPrice = 299m, DepositPaid = 150m, AssetLabel = "Lane 2", StaffNote = "Ortiz (safety)", Status = "Booked", Notes = "Discount source: Groupon - the balance is due at check-in." };
        var b3 = new Booking { PackageLabel = "Fan Favorite (SAMPLE)", LeadGuest = "Sample Delgado Party", PartySize = 5, SlotDate = new DateTimeOffset(today.AddDays(-3), now.Offset), SlotTimeNote = "09:00", Source = "Direct", TotalPrice = 4950m, DepositPaid = 4950m, AssetLabel = "Sherman tank + Lane 2", Status = "Completed" };
        await new JsonListStore<Booking>("bookings.json").SaveAsync(new List<Booking> { b1, b2, b3 });

        await new JsonListStore<Participant>("participants.json").SaveAsync(new List<Participant>
        {
            new() { BookingId = b1.Id, BookingLabel = "Carter 10:00", Name = "Sample Alex Carter", WaiverSigned = true, WaiverVersion = "v2026.1", SignedNote = "Paper waiver on file" },
            new() { BookingId = b1.Id, BookingLabel = "Carter 10:00", Name = "Sample Blair Carter", WaiverSigned = true, WaiverVersion = "v2026.1" },
            new() { BookingId = b2.Id, BookingLabel = "Nguyen 13:00", Name = "Sample Kim Nguyen", WaiverSigned = true, WaiverVersion = "v2026.1" },
            new() { BookingId = b2.Id, BookingLabel = "Nguyen 13:00", Name = "Sample Pat Nguyen", WaiverSigned = false, WaiverVersion = string.Empty, SignedNote = "NOT SIGNED - the gate flags this party before the 13:00 slot." },
            new() { BookingId = b2.Id, BookingLabel = "Nguyen 13:00", Name = "Sample Minor Nguyen", WaiverSigned = false, IsMinor = true, GuardianSigner = "Sample Kim Nguyen", SignedNote = "Minor - guardian signature still needed." },
        });

        await new JsonListStore<MediaItem>("media.json").SaveAsync(new List<MediaItem>
        {
            new() { BookingLabel = "Delgado Fan Favorite", GuestLabel = "Sample Delgado Party", Kind = "Video package", Price = 75m, EventDate = now.AddDays(-3), Captured = true, DeliveredDate = null, Notes = "Shot and owed - 3 days waiting, the follow-up clock is RED." },
            new() { BookingLabel = "Carter Tank Drive", GuestLabel = "Sample Carter Party", Kind = "Photo package", Price = 50m, EventDate = new DateTimeOffset(today, now.Offset), Captured = false },
        });

        await new JsonListStore<RangeGuest>("guests.json").SaveAsync(new List<RangeGuest>
        {
            new() { Name = "Sample Alex Carter", Contact = "sample guest - no real contact", OccasionNote = "Birthday", TotalVisits = 1, LastVisitDate = now, ReferralNote = "Brought 3 friends" },
            new() { Name = "Sample Rosa Delgado", Contact = "sample guest - no real contact", OccasionNote = "Corporate group", TotalVisits = 2, LastVisitDate = now.AddDays(-3) },
            new() { Name = "Sample Tom Beckett", Contact = "sample guest - no real contact", OccasionNote = "Bachelor party", TotalVisits = 1, LastVisitDate = now.AddDays(-200), ReferralNote = "Win-back candidate: 200 days since his visit - market to your own list before buying him back at a discount." },
        });

        await new JsonListStore<GiftCard>("giftcards.json").SaveAsync(new List<GiftCard>
        {
            new() { Code = "SAMPLE-GC-001", IssuedTo = "Sample Gift Buyer", Amount = 500m, Redeemed = 0m, IssuedDate = now.AddDays(-14), Notes = "Sample card - replace with real issuance." },
        });
    }
}
