using RallyPilot.Models;

namespace RallyPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var settingsStore = new RallySettingsStore();
        var settings = await settingsStore.LoadAsync();
        if (settings.SampleSeeded) return;

        var now = DateTimeOffset.Now;

        settings.BusinessName = "Sample Peak Adventures (SAMPLE)";
        settings.SampleSeeded = true;
        await settingsStore.SaveAsync(settings);

        await new JsonListStore<EventPackage>("packages.json").SaveAsync(new List<EventPackage>
        {
            new() { Name = "Corporate Range Day (SAMPLE)", Components = "Range rotations + safety officers + lunch block", PerHeadPrice = 185m, MinHeads = 10, CapacityNote = "Up to 60 in rotations", StaffingNote = "1 safety officer per lane" },
            new() { Name = "Tank Day Group (SAMPLE)", Components = "Tank drives + passenger rides + range block", PerGroupPrice = 3500m, MinHeads = 6, CapacityNote = "Groups of 6 per vehicle rotation", StaffingNote = "Driver-instructor per vehicle" },
        });

        await new JsonListStore<AddOnDef>("addons.json").SaveAsync(new List<AddOnDef>
        {
            new() { Name = "Extra laps", Price = 150m, PricingKind = "Per head" },
            new() { Name = "Passenger pass", Price = 50m, PricingKind = "Per head" },
            new() { Name = "Video package", Price = 75m, PricingKind = "Per head" },
            new() { Name = "Catering", Price = 28m, PricingKind = "Per head" },
        });

        var org = new Organizer { Company = "Sample Meridian Corp", ContactName = "Sample Dana Whitaker", Contact = "sample organizer - no real contact", TypicalSize = "40 heads", BooksAnnually = "Books annually - Q4 retreat", Notes = "Judges vendors on organization; rebooks the ones who make her look good." };
        await new JsonListStore<Organizer>("organizers.json").SaveAsync(new List<Organizer> { org });

        var quoteSent = new Quote
        {
            CompanyLabel = "Sample Harbor Logistics", OrganizerLabel = "Sample Lee Tran",
            PackageLabel = "Corporate Range Day (SAMPLE)", Heads = 25, EventDate = now.AddDays(45),
            DepositTerms = "50% deposit to book (sample terms)", Status = "Sent", SentDate = now.AddDays(-2),
            Notes = "Follow-up clock: orange at 2 days unanswered, red at 3.",
        };
        var quoteWon = new Quote
        {
            CompanyLabel = org.Company, OrganizerLabel = org.ContactName,
            PackageLabel = "Corporate Range Day (SAMPLE)", Heads = 40, EventDate = now.AddDays(12),
            DepositTerms = "50% deposit to book (sample terms)", Status = "Booked", SentDate = now.AddDays(-20),
        };
        await new JsonListStore<Quote>("quotes.json").SaveAsync(new List<Quote> { quoteSent, quoteWon });
        await new JsonListStore<QuoteLine>("quotelines.json").SaveAsync(new List<QuoteLine>
        {
            new() { QuoteId = quoteWon.Id, Label = "Corporate Range Day - per head", Qty = 40m, UnitPrice = 185m },
            new() { QuoteId = quoteWon.Id, Label = "Catering - per head", Qty = 40m, UnitPrice = 28m },
            new() { QuoteId = quoteWon.Id, Label = "Video package - per head", Qty = 10m, UnitPrice = 75m },
            new() { QuoteId = quoteSent.Id, Label = "Corporate Range Day - per head", Qty = 25m, UnitPrice = 185m },
        });

        var ev = new RallyEvent
        {
            QuoteId = quoteWon.Id, CompanyLabel = org.Company, OrganizerLabel = org.ContactName,
            OrganizerContact = org.Contact, QuoteRef = "Quote: Corporate Range Day x40 (sample ref)",
            EventDate = now.AddDays(12), ExpectedHeads = 40, ConfirmedHeads = 38,
            QuotedTotal = 9270m, DepositDue = 4635m, DepositPaid = 4635m, PaymentsReceived = 0m,
            BalanceDueDate = now.AddDays(2), Status = "Planning",
            SpecialRequirements = "Two vegetarian meals; awards at 16:30; bus transport arrives 08:30 (sample notes)",
            Notes = "Balance clock is ORANGE: the balance falls due 2 days from now, before the event.",
        };
        await new JsonListStore<RallyEvent>("events.json").SaveAsync(new List<RallyEvent> { ev });

        var roster = new List<RosterEntry>();
        for (var i = 1; i <= 40; i++)
        {
            roster.Add(new RosterEntry
            {
                EventId = ev.Id, EventLabel = $"{org.Company} {ev.EventDate:MMM d}",
                Name = $"Sample Participant {i:00}",
                WaiverSigned = i <= 31,
                WaiverVersion = i <= 31 ? "v2026.1" : string.Empty,
                Note = i <= 31 ? string.Empty : "On the chase list - reminder goes to the organizer.",
            });
        }
        await new JsonListStore<RosterEntry>("roster.json").SaveAsync(roster);

        await new JsonListStore<RunBlock>("runblocks.json").SaveAsync(new List<RunBlock>
        {
            new() { EventId = ev.Id, StartNote = "08:30", EndNote = "09:00", Title = "Arrival + check-in against the roster", Kind = "Briefing", StaffLabel = "All staff", AssetStatusNote = "Roster printed from RallyPilot" },
            new() { EventId = ev.Id, StartNote = "09:00", EndNote = "10:30", Title = "Range rotation A", Kind = "Activity", GroupLabel = "Group 1", StaffLabel = "Safety officer per lane" },
            new() { EventId = ev.Id, StartNote = "10:30", EndNote = "12:00", Title = "Range rotation B", Kind = "Activity", GroupLabel = "Group 2", StaffLabel = "Safety officer per lane" },
            new() { EventId = ev.Id, StartNote = "12:00", EndNote = "13:00", Title = "Lunch (catering)", Kind = "Meal", GroupLabel = "All", StaffLabel = "Host" },
            new() { EventId = ev.Id, StartNote = "13:00", EndNote = "16:30", Title = "Free rotations + add-on desk", Kind = "Activity", GroupLabel = "Open", StaffLabel = "Rotation staff", AssetStatusNote = "Check asset status before promising rotations" },
        });

        await new JsonListStore<AddOnSale>("addonsales.json").SaveAsync(new List<AddOnSale>
        {
            new() { EventId = ev.Id, EventLabel = ev.CompanyLabel, Label = "Video package (pre-sold)", Qty = 10m, UnitPrice = 75m, Date = now.AddDays(-2) },
        });
    }
}
