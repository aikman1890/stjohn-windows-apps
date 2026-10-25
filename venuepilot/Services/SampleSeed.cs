using VenuePilot.Models;
using Windows.Storage;

namespace VenuePilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("venue-seeded.flag") is not null) return;

        var store = new JsonListStore<VenueEvent>("events.json");
        if ((await store.LoadAsync()).Count == 0)
        {
            var now = DateTimeOffset.Now;
            var e1 = new VenueEvent
            {
                ClientNames = "Sample - Emma & Noah", Space = "Barn", EventType = "Wedding",
                EventDate = now.AddDays(24), Status = "Contracted", GuestCount = 140,
                PackageName = "Full Day - Peak", PackagePrice = 4500m,
                ContractDate = now.AddDays(-40), BalanceDueDate = now.AddDays(10),
                LoadIn = "Fri 4:00 PM", LoadOut = "Sun 11:00 AM",
                TimelineNotes = "Ceremony 5:00 PM on the grounds, reception in the barn (sample).",
            };
            var e2 = new VenueEvent
            {
                ClientNames = "Sample - Delgado Reunion", Space = "Grounds", EventType = "Reunion",
                EventDate = now.AddDays(38), Status = "Contracted", GuestCount = 85,
                PackageName = "Full Day - Peak", PackagePrice = 4500m,
                ContractDate = now.AddDays(-20), BalanceDueDate = now.AddDays(-6),
                Notes = "Balance overdue on purpose in the sample so the flag shows.",
            };
            var e3 = new VenueEvent
            {
                ClientNames = "Sample - Corporate Retreat Co", Space = "Hall", EventType = "Corporate",
                EventDate = now.AddDays(52), Status = "Contracted", GuestCount = 60,
                PackageName = "Half Day / Sunday", PackagePrice = 2200m,
                ContractDate = now.AddDays(-10), BalanceDueDate = now.AddDays(30),
            };
            var e4 = new VenueEvent
            {
                ClientNames = "Sample - Ava & Liam", Space = "Barn", EventType = "Wedding",
                EventDate = now.AddDays(66), Status = "Contracted", GuestCount = 120,
                PackageName = "Full Day - Peak", PackagePrice = 4500m,
                ContractDate = now.AddDays(-5), BalanceDueDate = now.AddDays(45),
            };
            var e5 = new VenueEvent
            {
                ClientNames = "Sample - Sofia & Mateo", Space = "Barn", EventType = "Wedding",
                EventDate = now.AddDays(80), Status = "Hold",
                HoldExpiry = now.AddDays(-2),
                Notes = "Hold expiry has passed in the sample - the flag is the point.",
            };
            var e6 = new VenueEvent
            {
                ClientNames = "Sample - Harper (inquiry)", Space = "Hall", EventType = "Birthday",
                EventDate = now.AddDays(45), Status = "Inquiry", Source = "Referral",
                LastContacted = now.AddDays(-12), TourDate = now.AddDays(-13),
                Notes = "Stale inquiry sample - untouched over 7 days.",
            };
            var e7 = new VenueEvent
            {
                ClientNames = "Sample - Quinn (inquiry)", Space = "Grounds", EventType = "Wedding",
                EventDate = now.AddDays(120), Status = "Inquiry", Source = "Drive-by",
                LastContacted = now.AddDays(-1),
            };
            var e8 = new VenueEvent
            {
                ClientNames = "Sample - The Patels", Space = "Barn", EventType = "Wedding",
                EventDate = now.AddDays(-14), Status = "Completed", GuestCount = 160,
                PackageName = "Full Day - Peak", PackagePrice = 4500m,
                ContractDate = now.AddDays(-120),
            };
            var events = new List<VenueEvent> { e1, e2, e3, e4, e5, e6, e7, e8 };
            await store.SaveAsync(events);

            await new JsonListStore<VenuePayment>("payments.json").SaveAsync(new List<VenuePayment>
            {
                new() { EventId = e1.Id, Date = now.AddDays(-40), Amount = 1000m, Kind = "Deposit", Note = "Sample deposit" },
                new() { EventId = e2.Id, Date = now.AddDays(-20), Amount = 1000m, Kind = "Deposit", Note = "Sample deposit" },
                new() { EventId = e3.Id, Date = now.AddDays(-10), Amount = 1100m, Kind = "Deposit", Note = "Sample deposit (50%)" },
                new() { EventId = e4.Id, Date = now.AddDays(-5), Amount = 1000m, Kind = "Deposit", Note = "Sample deposit" },
                new() { EventId = e8.Id, Date = now.AddDays(-120), Amount = 1000m, Kind = "Deposit", Note = "Sample deposit" },
                new() { EventId = e8.Id, Date = now.AddDays(-15), Amount = 3500m, Kind = "Final", Note = "Sample final settlement" },
            });

            await new JsonListStore<VendorEntry>("vendors.json").SaveAsync(new List<VendorEntry>
            {
                new() { EventId = e1.Id, Role = "Caterer", Name = "Sample Catering Co", Phone = "(555) 010-0400", ArrivalTime = "2:00 PM" },
                new() { EventId = e1.Id, Role = "DJ / Band", Name = "Sample DJ", Phone = "(555) 010-0401", ArrivalTime = "3:30 PM" },
                new() { EventId = e1.Id, Role = "Florist", Name = "Sample Florist", Phone = "(555) 010-0402", ArrivalTime = "1:00 PM" },
            });
        }
        await folder.CreateFileAsync("venue-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
