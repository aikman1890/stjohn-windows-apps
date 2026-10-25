using GuardPilot.Models;
using Windows.Storage;

namespace GuardPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("guard-seeded.flag") is not null) return;

        var siteStore = new JsonListStore<ClientSite>("sites.json");
        if ((await siteStore.LoadAsync()).Count > 0)
        {
            await folder.CreateFileAsync("guard-seeded.flag", CreationCollisionOption.ReplaceExisting);
            return;
        }

        var s1 = new ClientSite { Client = "Sample Client - Gulf Logistics", SiteName = "Warehouse Gate", Address = "100 Sample Industrial Pkwy", ContractRef = "CTR-S-101", BillRatePerHour = 28m, BillingTerms = "Net 30, monthly (sample)" };
        var s2 = new ClientSite { Client = "Sample Client - Harbor Condos HOA", SiteName = "Condo Patrol", Address = "200 Sample Harbor Dr", ContractRef = "CTR-S-102", BillRatePerHour = 24m };
        var s3 = new ClientSite { Client = "Sample Client - Northgate Retail", SiteName = "Parking Lot Post", Address = "300 Sample Northgate Blvd", ContractRef = "CTR-S-103", BillRatePerHour = 26m };
        var s4 = new ClientSite { Client = "Sample Client - MedPark Clinic", SiteName = "Clinic Night Post", Address = "400 Sample Medpark Way", ContractRef = "CTR-S-104", BillRatePerHour = 30m };
        await siteStore.SaveAsync(new List<ClientSite> { s1, s2, s3, s4 });

        var p1 = new Post { SiteId = s1.Id, SiteLabel = s1.DisplayLine, Name = "Gate Officer" };
        var p2 = new Post { SiteId = s2.Id, SiteLabel = s2.DisplayLine, Name = "Roving Patrol" };
        var p3 = new Post { SiteId = s3.Id, SiteLabel = s3.DisplayLine, Name = "Lot Officer" };
        var p4 = new Post { SiteId = s4.Id, SiteLabel = s4.DisplayLine, Name = "Night Officer" };
        await new JsonListStore<Post>("posts.json").SaveAsync(new List<Post> { p1, p2, p3, p4 });

        await new JsonListStore<PostOrderRevision>("orders.json").SaveAsync(new List<PostOrderRevision>
        {
            new()
            {
                PostId = p1.Id, PostLabel = p1.DisplayLine, Version = 1,
                EffectiveDate = DateTimeOffset.Now.AddDays(-60),
                OrdersText = "Remain at the gate; log every truck in and out; no gate left unattended (sample orders).",
                SiteInstructions = "Gate code is held by the site contact - never written in this app.",
                EmergencyContacts = "Site manager (sample) - see site file",
                EscalationChain = "Officer -> Supervisor -> Owner",
                Active = true,
            },
        });

        var g1 = new Guard { Name = "Sample Guard - J. Alvarez", Phone = "(555) 010-0501", LicenseNumber = "TX-SAMPLE-001", LicenseExpiry = DateTimeOffset.Now.AddDays(240) };
        var g2 = new Guard { Name = "Sample Guard - M. Chen", Phone = "(555) 010-0502", LicenseNumber = "TX-SAMPLE-002", LicenseExpiry = DateTimeOffset.Now.AddDays(12), Notes = "Licence renews soon - the dashboard warning is the demo." };
        var g3 = new Guard { Name = "Sample Guard - D. Okafor", Phone = "(555) 010-0503", LicenseNumber = "TX-SAMPLE-003", LicenseExpiry = DateTimeOffset.Now.AddDays(400) };
        var g4 = new Guard { Name = "Sample Guard - S. Novak", Phone = "(555) 010-0504", LicenseNumber = "TX-SAMPLE-004", LicenseExpiry = DateTimeOffset.Now.AddDays(90) };
        var g5 = new Guard { Name = "Sample Guard - R. Tran", Phone = "(555) 010-0505", LicenseNumber = "TX-SAMPLE-005", LicenseExpiry = DateTimeOffset.Now.AddDays(180) };
        var g6 = new Guard { Name = "Sample Guard - K. Brooks", Phone = "(555) 010-0506", LicenseNumber = "TX-SAMPLE-006", LicenseExpiry = DateTimeOffset.Now.AddDays(300), Status = "Inactive", Notes = "Sample inactive guard." };
        var guards = new List<Guard> { g1, g2, g3, g4, g5, g6 };
        await new JsonListStore<Guard>("guards.json").SaveAsync(guards);

        // Current week (Monday start).
        var today = DateTimeOffset.Now.Date;
        var monday = today.AddDays(-(((int)DateTimeOffset.Now.DayOfWeek + 6) % 7));
        DateTimeOffset Day(int offset, int hour = 0) => new(monday.AddDays(offset).AddHours(hour), TimeSpan.Zero);
        var shifts = new List<Shift>
        {
            new() { PostId = p1.Id, PostLabel = p1.DisplayLine, SiteLabel = s1.DisplayLine, BillRatePerHour = 28m, Date = Day(0), StartHour = 18m, EndHour = 30m, GuardId = g1.Id, GuardLabel = g1.Name, ActualHours = 12m },
            new() { PostId = p1.Id, PostLabel = p1.DisplayLine, SiteLabel = s1.DisplayLine, BillRatePerHour = 28m, Date = Day(1), StartHour = 18m, EndHour = 30m, GuardId = g1.Id, GuardLabel = g1.Name, ActualHours = 12m },
            new() { PostId = p2.Id, PostLabel = p2.DisplayLine, SiteLabel = s2.DisplayLine, BillRatePerHour = 24m, Date = Day(1), StartHour = 20m, EndHour = 28m, GuardId = g2.Id, GuardLabel = g2.Name, ActualHours = 8m },
            new() { PostId = p3.Id, PostLabel = p3.DisplayLine, SiteLabel = s3.DisplayLine, BillRatePerHour = 26m, Date = Day(2), StartHour = 16m, EndHour = 24m, GuardId = g3.Id, GuardLabel = g3.Name },
            new() { PostId = p4.Id, PostLabel = p4.DisplayLine, SiteLabel = s4.DisplayLine, BillRatePerHour = 30m, Date = Day(3), StartHour = 22m, EndHour = 30m, GuardId = string.Empty, GuardLabel = string.Empty, Notes = "Sample UNFILLED shift - the red flag on the board." },
            new() { PostId = p1.Id, PostLabel = p1.DisplayLine, SiteLabel = s1.DisplayLine, BillRatePerHour = 28m, Date = Day(4), StartHour = 18m, EndHour = 30m, GuardId = g4.Id, GuardLabel = g4.Name },
            new() { PostId = p2.Id, PostLabel = p2.DisplayLine, SiteLabel = s2.DisplayLine, BillRatePerHour = 24m, Date = Day(5), StartHour = 20m, EndHour = 28m, GuardId = g5.Id, GuardLabel = g5.Name },
        };
        await new JsonListStore<Shift>("shifts.json").SaveAsync(shifts);

        await new JsonListStore<Incident>("incidents.json").SaveAsync(new List<Incident>
        {
            new()
            {
                When = DateTimeOffset.Now.AddDays(-2).AddHours(-3),
                SiteLabel = s1.DisplayLine, PostLabel = p1.DisplayLine, GuardLabel = g1.Name,
                Type = "Trespass", Narrative = "Sample narrative: two individuals at the fence line, left when challenged; no damage.",
                FollowUp = "Send incident summary to the client contact (sample)",
                FollowUpOwner = "Owner", FollowUpDue = DateTimeOffset.Now.AddDays(-1), FollowUpDone = false,
            },
            new()
            {
                When = DateTimeOffset.Now.AddDays(-1).AddHours(-5),
                SiteLabel = s3.DisplayLine, PostLabel = p3.DisplayLine, GuardLabel = g3.Name,
                Type = "Property damage", Narrative = "Sample narrative: shopping cart into a parked car; photos taken; store manager notified.",
                FollowUp = "Attach photos to the client email", FollowUpOwner = "Owner",
                FollowUpDue = DateTimeOffset.Now.AddDays(2), FollowUpDone = false,
            },
        });

        await new JsonListStore<DarEntry>("dars.json").SaveAsync(new List<DarEntry>
        {
            new() { Date = DateTimeOffset.Now.AddDays(-1), PostLabel = p1.DisplayLine, GuardLabel = g1.Name, Summary = "All quiet; 14 trucks logged in/out (sample DAR)." },
        });
        await folder.CreateFileAsync("guard-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
