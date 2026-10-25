using EntityPilot.Models;
using Windows.Storage;

namespace EntityPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("entity-seeded.flag") is not null) return;

        var entityStore = new JsonListStore<EntityRecord>("entities.json");
        if ((await entityStore.LoadAsync()).Count > 0)
        {
            await folder.CreateFileAsync("entity-seeded.flag", CreationCollisionOption.ReplaceExisting);
            return;
        }

        // FICTIONAL sample entities only - never the owner's real companies.
        var e1 = new EntityRecord
        {
            LegalName = "Bayou Sample Holdings LLC (SAMPLE)", Type = "LLC", State = "TX",
            FormationDate = DateTimeOffset.Now.AddYears(-4), FileNumber = "SAMPLE-001",
            Ein = "00-0000000", FiscalYearEnd = "12-31", Status = "Active",
            StatusConfirmedDate = DateTimeOffset.Now.AddDays(-20),
            RegisteredAgentName = "Sample Agent Services", AgentAddress = "100 Sample St, Austin, TX (sample)",
            AgentFeeNote = "$129/yr (sample)", AgentRenewalDate = DateTimeOffset.Now.AddMonths(5),
            PortalReference = "Texas Comptroller Webfile - username on file in vault (sample)",
        };
        var e2 = new EntityRecord
        {
            LegalName = "High Plains Sample LLC (SAMPLE)", Type = "LLC", State = "WY",
            FormationDate = DateTimeOffset.Now.AddYears(-2), FileNumber = "SAMPLE-002",
            Ein = "00-0000000", FiscalYearEnd = "12-31", Status = "Unknown",
            RegisteredAgentName = "Sample Wyoming Agent", AgentAddress = "30 Sample St, Sheridan, WY (sample)",
            AgentFeeNote = "$99/yr (sample)", AgentRenewalDate = DateTimeOffset.Now.AddDays(-12),
        };
        var e3 = new EntityRecord
        {
            LegalName = "First State Sample Inc (SAMPLE)", Type = "C-corp", State = "DE",
            FormationDate = DateTimeOffset.Now.AddYears(-3), FileNumber = "SAMPLE-003",
            Ein = "00-0000000", FiscalYearEnd = "12-31", Status = "Active",
            StatusConfirmedDate = DateTimeOffset.Now.AddDays(-45),
            RegisteredAgentName = "Sample Delaware Agent", AgentRenewalDate = DateTimeOffset.Now.AddMonths(8),
        };
        var e4 = new EntityRecord
        {
            LegalName = "Lone Star Sample Ventures LP (SAMPLE)", Type = "LP", State = "TX",
            FormationDate = DateTimeOffset.Now.AddYears(-1), FileNumber = "SAMPLE-004",
            Ein = "00-0000000", FiscalYearEnd = "12-31", Status = "Active",
            StatusConfirmedDate = DateTimeOffset.Now.AddDays(-10),
            RegisteredAgentName = "Sample Agent Services", AgentRenewalDate = DateTimeOffset.Now.AddMonths(2),
        };
        var entities = new List<EntityRecord> { e1, e2, e3, e4 };
        await entityStore.SaveAsync(entities);

        await new JsonListStore<Obligation>("obligations.json").SaveAsync(new List<Obligation>
        {
            new() { EntityId = e1.Id, EntityLabel = e1.LegalName, Kind = "Annual report", Title = "TX franchise tax report (sample template)", DueDate = DateTimeOffset.Now.AddDays(9), RecursYearly = true },
            new() { EntityId = e2.Id, EntityLabel = e2.LegalName, Kind = "Agent renewal", Title = "Registered-agent renewal (sample)", DueDate = DateTimeOffset.Now.AddDays(-12), RecursYearly = true },
            new() { EntityId = e3.Id, EntityLabel = e3.LegalName, Kind = "Franchise tax", Title = "DE franchise tax (sample template)", DueDate = DateTimeOffset.Now.AddDays(68), RecursYearly = true },
            new() { EntityId = e4.Id, EntityLabel = e4.LegalName, Kind = "Annual report", Title = "TX franchise tax report (sample template)", DueDate = DateTimeOffset.Now.AddDays(120), RecursYearly = true },
        });

        await new JsonListStore<OwnershipStake>("ownership.json").SaveAsync(new List<OwnershipStake>
        {
            new() { EntityId = e1.Id, EntityLabel = e1.LegalName, OwnerName = "Sample Owner A", Class = "Membership", Percent = 60m, Contribution = 6000m, EffectiveDate = DateTimeOffset.Now.AddYears(-4) },
            new() { EntityId = e1.Id, EntityLabel = e1.LegalName, OwnerName = "Sample Owner B", Class = "Membership", Percent = 40m, Contribution = 4000m, EffectiveDate = DateTimeOffset.Now.AddYears(-4) },
            new() { EntityId = e2.Id, EntityLabel = e2.LegalName, OwnerName = "Sample Owner A", Class = "Membership", Percent = 100m, Contribution = 1000m, EffectiveDate = DateTimeOffset.Now.AddYears(-2) },
        });

        await new JsonListStore<OfficerEntry>("officers.json").SaveAsync(new List<OfficerEntry>
        {
            new() { EntityId = e1.Id, EntityLabel = e1.LegalName, Name = "Sample Owner A", Role = "Manager", AppointedDate = DateTimeOffset.Now.AddYears(-4) },
            new() { EntityId = e3.Id, EntityLabel = e3.LegalName, Name = "Sample Owner A", Role = "President", AppointedDate = DateTimeOffset.Now.AddYears(-3) },
        });

        await new JsonListStore<IntercompanyEntry>("intercompany.json").SaveAsync(new List<IntercompanyEntry>
        {
            new() { FromEntityId = e1.Id, FromLabel = e1.LegalName, ToEntityId = e2.Id, ToLabel = e2.LegalName, Kind = "Loan", Amount = 15000m, Date = DateTimeOffset.Now.AddMonths(-6), TermsNote = "Sample note - repay on demand" },
            new() { FromEntityId = e2.Id, FromLabel = e2.LegalName, ToEntityId = e1.Id, ToLabel = e1.LegalName, Kind = "Repayment", Amount = 2500m, Date = DateTimeOffset.Now.AddMonths(-2), TermsNote = "Partial repayment (sample)" },
        });

        await folder.CreateFileAsync("entity-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
