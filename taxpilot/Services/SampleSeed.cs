using TaxPilot.Models;
using Windows.Storage;

namespace TaxPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("tax-seeded.flag") is not null) return;

        var clientStore = new JsonListStore<TaxClient>("clients.json");
        if ((await clientStore.LoadAsync()).Count > 0)
        {
            await folder.CreateFileAsync("tax-seeded.flag", CreationCollisionOption.ReplaceExisting);
            return;
        }

        var year = DateTime.Now.Year - 1;
        var specs = new (string Name, string Kind, string Entity, string Status, decimal Fee, decimal Paid, bool Ext, string Carry)[]
        {
            ("Sample Client - Garcia, M.", "Individual", "1040", "Accepted", 350m, 350m, false, "K-1 expected from brother's partnership"),
            ("Sample Client - Henderson, J.", "Individual", "1040", "In prep", 425m, 200m, false, ""),
            ("Sample Client - Okafor, A.", "Individual", "1040", "Docs pending", 300m, 0m, false, "New baby in November"),
            ("Sample Client - Bluebonnet Bakery LLC", "Business", "1120-S", "In review", 850m, 425m, false, ""),
            ("Sample Client - Tran Nail Spa Inc", "Business", "1120", "Ready to file", 900m, 900m, false, ""),
            ("Sample Client - Pinewood HOA", "Business", "1120-H", "Not started", 275m, 0m, false, "Reserve study due from manager"),
            ("Sample Client - Alvarez Family Trust", "Business", "1041", "Extended", 600m, 300m, true, ""),
            ("Sample Client - Riverbend Partnership", "Business", "1065", "Filed", 1100m, 1100m, false, "One partner always late with K-1 data"),
            ("Sample Client - Nguyen, T.", "Individual", "1040", "Rejected-fix", 375m, 375m, false, ""),
            ("Sample Client - Community Food Shelf", "Business", "990", "Docs pending", 500m, 0m, false, ""),
            ("Sample Client - Whitfield, R.", "Individual", "1040", "Done", 325m, 325m, false, "Rental property sold in June - expect 1099-S"),
            ("Sample Client - Kowalski, S.", "Individual", "1040", "Not started", 290m, 0m, false, ""),
        };

        var clients = new List<TaxClient>();
        var engagements = new List<Engagement>();
        var docs = new List<DocItem>();
        var efile = new List<EfileLog>();
        var contacts = new List<ContactLogEntry>();

        foreach (var s in specs)
        {
            var client = new TaxClient
            {
                Name = s.Name, Kind = s.Kind, EntityType = s.Entity,
                Email = "sample@example.invalid", CarryNotes = s.Carry,
            };
            clients.Add(client);
            var eng = new Engagement
            {
                ClientId = client.Id, ClientLabel = client.Name, TaxYear = year,
                EntityType = s.Entity, Status = s.Status, FeeQuoted = s.Fee, FeePaid = s.Paid,
                Extended = s.Ext, ExtensionFiledDate = s.Ext ? DateTimeOffset.Now.AddDays(-30) : null,
                LastContacted = DateTimeOffset.Now.AddDays(-3),
            };
            engagements.Add(eng);

            if (s.Name.Contains("Okafor"))
            {
                docs.Add(new DocItem { EngagementId = eng.Id, ClientLabel = client.Name, Label = "W-2", Received = true, ReceivedDate = DateTimeOffset.Now.AddDays(-9) });
                docs.Add(new DocItem { EngagementId = eng.Id, ClientLabel = client.Name, Label = "1099s" });
                docs.Add(new DocItem { EngagementId = eng.Id, ClientLabel = client.Name, Label = "Prior-year return", Received = true, ReceivedDate = DateTimeOffset.Now.AddDays(-9) });
            }
            if (s.Name.Contains("Food Shelf"))
            {
                docs.Add(new DocItem { EngagementId = eng.Id, ClientLabel = client.Name, Label = "Organization books" });
                docs.Add(new DocItem { EngagementId = eng.Id, ClientLabel = client.Name, Label = "Board list", Received = true, ReceivedDate = DateTimeOffset.Now.AddDays(-5) });
            }
            if (s.Name.Contains("Nguyen"))
            {
                efile.Add(new EfileLog
                {
                    EngagementId = eng.Id, ClientLabel = client.Name,
                    SubmittedAt = DateTimeOffset.Now.AddDays(-4), Method = "Prep software e-file (sample)",
                    Result = "Rejected", RejectCode = "SAMPLE-R0000",
                    Note = "Name/SSN mismatch per the sample rejection - fixed and ready to resubmit.",
                });
            }
            if (s.Name.Contains("Garcia"))
            {
                efile.Add(new EfileLog
                {
                    EngagementId = eng.Id, ClientLabel = client.Name,
                    SubmittedAt = DateTimeOffset.Now.AddDays(-12), Method = "Prep software e-file (sample)",
                    Result = "Accepted",
                });
            }
            if (s.Name.Contains("Henderson"))
            {
                contacts.Add(new ContactLogEntry
                {
                    ClientId = client.Id, ClientLabel = client.Name,
                    Date = DateTimeOffset.Now.AddDays(-2), Who = "Client (phone)",
                    Summary = "Asked about the missing 1099-INT (sample entry)",
                    Promised = "Bank letter by Friday",
                });
            }
        }

        await clientStore.SaveAsync(clients);
        await new JsonListStore<Engagement>("engagements.json").SaveAsync(engagements);
        await new JsonListStore<DocItem>("docs.json").SaveAsync(docs);
        await new JsonListStore<EfileLog>("efile.json").SaveAsync(efile);
        await new JsonListStore<ContactLogEntry>("contacts.json").SaveAsync(contacts);
        await folder.CreateFileAsync("tax-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
