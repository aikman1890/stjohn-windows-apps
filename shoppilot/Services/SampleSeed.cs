using ShopPilot.Models;
using Windows.Storage;

namespace ShopPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("shop-seeded.flag") is not null) return;

        var customerStore = new JsonListStore<ShopCustomer>("customers.json");
        if ((await customerStore.LoadAsync()).Count == 0)
        {
            var c1 = new ShopCustomer { Name = "Bayou Fabrication (SAMPLE SHOP)", Contact = "Sample Owner", Notes = "All sample data - fictional." };
            var c2 = new ShopCustomer { Name = "Sample Customer - Gulf Pumps LLC" };
            var c3 = new ShopCustomer { Name = "Sample Customer - Delta Ag Parts" };
            await customerStore.SaveAsync(new List<ShopCustomer> { c1, c2, c3 });

            var q1 = new ShopQuote
            {
                Number = "Q-1001", CustomerId = c2.Id, CustomerLabel = c2.Name,
                PartDescription = "Pump mounting bracket", PartNumber = "GP-BRK-220",
                Material = "A36 steel", Quantity = 50, QuotedPrice = 2450m, LeadTimeDays = 10,
                EstMaterialCost = 700m, EstLaborHours = 12m, EstOutsideCost = 300m,
                Status = "Won", Date = DateTimeOffset.Now.AddDays(-12),
            };
            var q2 = new ShopQuote
            {
                Number = "Q-1002", CustomerId = c3.Id, CustomerLabel = c3.Name,
                PartDescription = "PTO shaft guard", PartNumber = "DA-SG-118",
                Material = "11 ga HR steel", Quantity = 200, QuotedPrice = 3900m, LeadTimeDays = 14,
                EstMaterialCost = 1500m, EstLaborHours = 20m, EstOutsideCost = 0m,
                Status = "Won", Date = DateTimeOffset.Now.AddDays(-9),
            };
            var q3 = new ShopQuote
            {
                Number = "Q-1003", CustomerId = c2.Id, CustomerLabel = c2.Name,
                PartDescription = "Valve body prototype", PartNumber = "GP-VB-009",
                Material = "6061 aluminum", Quantity = 2, QuotedPrice = 980m, LeadTimeDays = 7,
                EstMaterialCost = 180m, EstLaborHours = 6m, EstOutsideCost = 0m,
                Status = "Lost", LossReason = "Price - went to a bigger shop (sample)",
                Date = DateTimeOffset.Now.AddDays(-6),
            };
            var q4 = new ShopQuote
            {
                Number = "Q-1004", CustomerId = c3.Id, CustomerLabel = c3.Name,
                PartDescription = "Hitch plate run", PartNumber = "DA-HP-340",
                Material = "A572-50", Quantity = 75, QuotedPrice = 3100m, LeadTimeDays = 12,
                EstMaterialCost = 1100m, EstLaborHours = 14m, EstOutsideCost = 250m,
                Status = "Open", Date = DateTimeOffset.Now.AddDays(-3),
            };
            var q5 = new ShopQuote
            {
                Number = "Q-1005", CustomerId = c2.Id, CustomerLabel = c2.Name,
                PartDescription = "Impeller hub rework", PartNumber = "GP-IH-077",
                Material = "316 SS", Quantity = 12, QuotedPrice = 1680m, LeadTimeDays = 5,
                EstMaterialCost = 420m, EstLaborHours = 8m, EstOutsideCost = 0m,
                Status = "Open", Date = DateTimeOffset.Now.AddDays(-1),
            };
            await new JsonListStore<ShopQuote>("quotes.json").SaveAsync(new List<ShopQuote> { q1, q2, q3, q4, q5 });

            var j1 = new ShopJob
            {
                JobNumber = "J-2001", QuoteId = q1.Id, CustomerId = c2.Id, CustomerLabel = c2.Name,
                PartDescription = q1.PartDescription, PartNumber = q1.PartNumber, Material = q1.Material,
                MaterialCertRef = "Mill cert filed in Job J-2001 folder (sample)", MaterialCost = 735m,
                Quantity = 50, QuotedPrice = 2450m, DueDate = DateTimeOffset.Now.AddDays(2),
                CustomerPO = "PO-8841 (sample)", WorkCenter = "Mill", Priority = 1, Status = "In process",
            };
            var j2 = new ShopJob
            {
                JobNumber = "J-2002", QuoteId = q2.Id, CustomerId = c3.Id, CustomerLabel = c3.Name,
                PartDescription = q2.PartDescription, PartNumber = q2.PartNumber, Material = q2.Material,
                MaterialCost = 1520m, Quantity = 200, QuotedPrice = 3900m,
                DueDate = DateTimeOffset.Now.AddDays(6), CustomerPO = "PO-2210 (sample)",
                WorkCenter = "Saw", Priority = 2, Status = "Open",
            };
            var j3 = new ShopJob
            {
                JobNumber = "J-1998", QuoteId = string.Empty, CustomerId = c2.Id, CustomerLabel = c2.Name,
                PartDescription = "Flange adapter (closed sample)", PartNumber = "GP-FA-150",
                Material = "1018 CR steel", MaterialCost = 610m, Quantity = 40, QtyShipped = 40,
                QuotedPrice = 1900m, DueDate = DateTimeOffset.Now.AddDays(-4),
                CustomerPO = "PO-8790 (sample)", WorkCenter = "Shipping", Priority = 5,
                Status = "Closed", ShipDate = DateTimeOffset.Now.AddDays(-3), InvoiceRef = "INV-311 (sample)",
            };
            await new JsonListStore<ShopJob>("jobs.json").SaveAsync(new List<ShopJob> { j1, j2, j3 });

            await new JsonListStore<JobOperation>("operations.json").SaveAsync(new List<JobOperation>
            {
                new() { JobId = j1.Id, Seq = 1, Name = "Saw cut", WorkCenter = "Saw", PlannedHours = 2m, ActualHours = 2.5m, Done = true, SignOff = "JT (sample)", SignDate = DateTimeOffset.Now.AddDays(-2) },
                new() { JobId = j1.Id, Seq = 2, Name = "Mill - face & profile", WorkCenter = "Mill", PlannedHours = 6m, ActualHours = 3m, Done = false },
                new() { JobId = j1.Id, Seq = 3, Name = "Deburr", WorkCenter = "Inspection", PlannedHours = 1m, ActualHours = 0m, Done = false },
                new() { JobId = j1.Id, Seq = 4, Name = "Inspect & measure", WorkCenter = "Inspection", PlannedHours = 1m, ActualHours = 0m, Done = false },
                new() { JobId = j1.Id, Seq = 5, Name = "Pack & ship", WorkCenter = "Shipping", PlannedHours = 1m, ActualHours = 0m, Done = false },
                new() { JobId = j3.Id, Seq = 1, Name = "Lathe - turn", WorkCenter = "Lathe", PlannedHours = 8m, ActualHours = 10.5m, Done = true, SignOff = "JT (sample)", SignDate = DateTimeOffset.Now.AddDays(-4) },
                new() { JobId = j3.Id, Seq = 2, Name = "Inspect & measure", WorkCenter = "Inspection", PlannedHours = 1m, ActualHours = 1m, Done = true, SignOff = "JT (sample)", SignDate = DateTimeOffset.Now.AddDays(-4) },
            });

            await new JsonListStore<OutsidePO>("outsidepos.json").SaveAsync(new List<OutsidePO>
            {
                new()
                {
                    JobId = j1.Id, JobLabel = $"{j1.JobNumber} - {j1.PartDescription}",
                    Vendor = "Sample Plating Co", Service = "Zinc plate", PONumber = "OS-501",
                    Cost = 310m, SentDate = DateTimeOffset.Now.AddDays(-1),
                    PromisedDate = DateTimeOffset.Now.AddDays(3), CertRef = "Plating cert to be filed on return (sample)",
                },
            });
        }
        await folder.CreateFileAsync("shop-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
