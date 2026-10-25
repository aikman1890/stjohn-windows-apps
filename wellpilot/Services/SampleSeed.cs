using WellPilot.Models;
using Windows.Storage;

namespace WellPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("well-seeded.flag") is not null) return;

        var wellStore = new JsonListStore<Well>("wells.json");
        if ((await wellStore.LoadAsync()).Count == 0)
        {
            var w1 = new Well
            {
                LeaseName = "Sample Lease - Hart Unit", WellName = "Hart #1",
                ApiNumber = "42-000-00000 (sample)", County = "Sample", State = "TX",
                Operator = "Sample Operating Co.", Status = "Producing",
                FirstProductionDate = DateTimeOffset.Now.AddYears(-6),
                Notes = "Fictional sample well.",
            };
            var w2 = new Well
            {
                LeaseName = "Sample Lease - Hart Unit", WellName = "Hart #2",
                ApiNumber = "42-000-00001 (sample)", County = "Sample", State = "TX",
                Operator = "Sample Operating Co.", Status = "Producing",
                FirstProductionDate = DateTimeOffset.Now.AddYears(-5),
            };
            var w3 = new Well
            {
                LeaseName = "Sample Lease - Mesa", WellName = "Mesa #1",
                ApiNumber = "42-000-00002 (sample)", County = "Sample", State = "TX",
                Operator = "Sample Operating Co.", Status = "Shut-in",
                Notes = "Shut-in sample - awaiting workover decision.",
            };
            await wellStore.SaveAsync(new List<Well> { w1, w2, w3 });

            var oA = new InterestOwner { Name = "Sample Royalty Owner A" };
            var oB = new InterestOwner { Name = "Sample ORRI Owner B" };
            var oC = new InterestOwner { Name = "Sample WI Partner C" };
            var oD = new InterestOwner { Name = "Sample WI Partner D" };
            var oE = new InterestOwner { Name = "Sample Royalty Owner E" };
            var oF = new InterestOwner { Name = "Sample WI Partner F" };
            await new JsonListStore<InterestOwner>("owners.json")
                .SaveAsync(new List<InterestOwner> { oA, oB, oC, oD, oE, oF });

            var eff = DateTimeOffset.Now.AddMonths(-3);
            await new JsonListStore<DeckEntry>("deck.json").SaveAsync(new List<DeckEntry>
            {
                new() { WellId = w1.Id, OwnerId = oA.Id, OwnerLabel = oA.Name, InterestType = "Royalty", DecimalInterest = 0.12500000m, EffectiveDate = eff, Version = 1 },
                new() { WellId = w1.Id, OwnerId = oB.Id, OwnerLabel = oB.Name, InterestType = "Overriding royalty", DecimalInterest = 0.03125000m, EffectiveDate = eff, Version = 1 },
                new() { WellId = w1.Id, OwnerId = oC.Id, OwnerLabel = oC.Name, InterestType = "Working interest", DecimalInterest = 0.50000000m, EffectiveDate = eff, Version = 1 },
                new() { WellId = w1.Id, OwnerId = oD.Id, OwnerLabel = oD.Name, InterestType = "Working interest", DecimalInterest = 0.34375000m, EffectiveDate = eff, Version = 1 },
                new() { WellId = w2.Id, OwnerId = oE.Id, OwnerLabel = oE.Name, InterestType = "Royalty", DecimalInterest = 0.18750000m, EffectiveDate = eff, Version = 1 },
                new() { WellId = w2.Id, OwnerId = oF.Id, OwnerLabel = oF.Name, InterestType = "Working interest", DecimalInterest = 0.81250000m, EffectiveDate = eff, Version = 1 },
            });

            var month = DateTimeOffset.Now.AddMonths(-1).ToString("yyyy-MM");
            await new JsonListStore<ProductionMonth>("production.json").SaveAsync(new List<ProductionMonth>
            {
                new()
                {
                    WellId = w1.Id, WellLabel = w1.DisplayLine, Month = month,
                    OilBbl = 412.5m, GasMcf = 890m, GrossRevenue = 11800m,
                    SeveranceTax = 900m, Deductions = 900m,
                    PurchaserRef = "Sample purchaser stmt #S-1001",
                    Notes = "Net revenue $10,000 - ties to the seeded distribution run.",
                },
            });
            await new JsonListStore<WellExpense>("expenses.json").SaveAsync(new List<WellExpense>
            {
                new() { WellId = w1.Id, WellLabel = w1.DisplayLine, Month = month, Category = "Labor", Amount = 900m, Vendor = "Sample pumper" },
                new() { WellId = w1.Id, WellLabel = w1.DisplayLine, Month = month, Category = "Chemicals", Amount = 650m, Vendor = "Sample chemical co" },
                new() { WellId = w1.Id, WellLabel = w1.DisplayLine, Month = month, Category = "Hauling", Amount = 850m, Vendor = "Sample hauling" },
            });

            var run = new DistributionRun
            {
                WellId = w1.Id, WellLabel = w1.DisplayLine, Month = month,
                RunDate = DateTimeOffset.Now.AddDays(-2),
                NetRevenue = 10000m, TotalExpenses = 2400m,
                DeckSnapshot = "Deck as of sample seed: RI 0.12500000, ORRI 0.03125000, WI 0.50000000 + 0.34375000 (all v1).",
            };
            await new JsonListStore<DistributionRun>("runs.json").SaveAsync(new List<DistributionRun> { run });
            await new JsonListStore<StatementLine>("lines.json").SaveAsync(new List<StatementLine>
            {
                new() { RunId = run.Id, OwnerId = oA.Id, OwnerLabel = oA.Name, InterestType = "Royalty", DecimalUsed = 0.12500000m, RevenueCredit = 1250.00m, JibCharge = 0m },
                new() { RunId = run.Id, OwnerId = oB.Id, OwnerLabel = oB.Name, InterestType = "Overriding royalty", DecimalUsed = 0.03125000m, RevenueCredit = 312.50m, JibCharge = 0m },
                new() { RunId = run.Id, OwnerId = oC.Id, OwnerLabel = oC.Name, InterestType = "Working interest", DecimalUsed = 0.50000000m, RevenueCredit = 5000.00m, JibCharge = 1422.22m },
                new() { RunId = run.Id, OwnerId = oD.Id, OwnerLabel = oD.Name, InterestType = "Working interest", DecimalUsed = 0.34375000m, RevenueCredit = 3437.50m, JibCharge = 977.78m },
            });

            await new JsonListStore<SuspenseEntry>("suspense.json").SaveAsync(new List<SuspenseEntry>
            {
                new()
                {
                    OwnerId = oB.Id, OwnerLabel = oB.Name, Date = DateTimeOffset.Now.AddDays(-20),
                    Amount = 143.20m, Reason = "Minimum-royalty threshold not met (sample)",
                },
            });

            await new JsonListStore<ComplianceDate>("calendar.json").SaveAsync(new List<ComplianceDate>
            {
                new() { Title = "Severance tax filing (sample - user-maintained)", Category = "Tax", DueDate = DateTimeOffset.Now.AddDays(12), Recurrence = "Monthly" },
                new() { Title = "Purchaser statement reconciliation", Category = "Reconciliation", DueDate = DateTimeOffset.Now.AddDays(4), Recurrence = "Monthly" },
                new() { Title = "State production report (sample)", Category = "State report", DueDate = DateTimeOffset.Now.AddDays(20), Recurrence = "Monthly" },
            });
        }
        await folder.CreateFileAsync("well-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
