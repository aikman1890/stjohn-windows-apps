using RemodelPilot.Models;
using Windows.Storage;

namespace RemodelPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("remodel-seeded.flag") is not null) return;

        var jobStore = new JsonListStore<Job>("jobs.json");
        if ((await jobStore.LoadAsync()).Count == 0)
        {
            var job = new Job
            {
                ClientName = "Sample Client - Kitchen Remodel",
                Address = "123 Sample Street",
                ContractType = "Fixed price",
                Status = "In progress",
                StartDate = DateTimeOffset.Now.AddDays(-21),
                Notes = "SAMPLE job - delete from the Jobs page.",
            };
            var est = new EstimateVersion
            {
                JobId = job.Id,
                JobLabel = job.DisplayLine,
                Version = 1,
                Status = "Won",
                Date = DateTimeOffset.Now.AddDays(-28),
                Lines = new List<EstimateLine>
                {
                    new() { Category = "Demo", Description = "Remove existing kitchen", Cost = 1800m, MarkupPercent = 20m },
                    new() { Category = "Cabinets", Description = "Shaker cabinets, installed", Cost = 9500m, MarkupPercent = 20m },
                    new() { Category = "Countertops", Description = "Quartz, 42 sq ft", Cost = 4200m, MarkupPercent = 20m },
                    new() { Category = "Electrical", Description = "Rewire + cans + undercabinet", Cost = 2600m, MarkupPercent = 20m },
                    new() { Category = "Plumbing", Description = "Sink, disposal, gas line move", Cost = 1900m, MarkupPercent = 20m },
                    new() { Category = "Flooring", Description = "LVP throughout kitchen", Cost = 3100m, MarkupPercent = 20m },
                },
            };
            job.BaseContractPrice = est.TotalPrice;
            job.WinningVersionId = est.Id;
            await jobStore.SaveAsync(new List<Job> { job });
            await new JsonListStore<EstimateVersion>("estimates.json").SaveAsync(new List<EstimateVersion> { est });
            await new JsonListStore<ChangeOrder>("changeorders.json").SaveAsync(new List<ChangeOrder>
            {
                new()
                {
                    JobId = job.Id, JobLabel = job.DisplayLine, Number = "CO-1",
                    Description = "Add pantry pull-outs (approved sample)",
                    Category = "Cabinets", Price = 640m, Status = "Approved",
                    SentDate = DateTimeOffset.Now.AddDays(-10), DecisionDate = DateTimeOffset.Now.AddDays(-9),
                    Method = "In person", ApprovedBy = "Sample Client",
                },
                new()
                {
                    JobId = job.Id, JobLabel = job.DisplayLine, Number = "CO-2",
                    Description = "Heated floor upgrade (declined sample - note it stays on file)",
                    Category = "Flooring", Price = 1850m, Status = "Declined",
                    SentDate = DateTimeOffset.Now.AddDays(-6), DecisionDate = DateTimeOffset.Now.AddDays(-5),
                    Method = "Email", ApprovedBy = "Sample Client",
                },
                new()
                {
                    JobId = job.Id, JobLabel = job.DisplayLine, Number = "CO-3",
                    Description = "Under-cabinet lighting upgrade (awaiting approval sample)",
                    Category = "Electrical", Price = 480m, Status = "Sent",
                    SentDate = DateTimeOffset.Now.AddDays(-2),
                },
            });
            await new JsonListStore<ActualCost>("actuals.json").SaveAsync(new List<ActualCost>
            {
                new() { JobId = job.Id, Category = "Demo", Description = "Dumpster + labor", Amount = 1650m, Date = DateTimeOffset.Now.AddDays(-20) },
                new() { JobId = job.Id, Category = "Cabinets", Description = "Cabinet deposit", Amount = 5200m, Date = DateTimeOffset.Now.AddDays(-14), Vendor = "Sample Cabinet Co" },
                new() { JobId = job.Id, Category = "Electrical", Description = "Rough-in materials", Amount = 2900m, Date = DateTimeOffset.Now.AddDays(-7), Vendor = "Sample Electric Supply" },
            });
            await new JsonListStore<Draw>("draws.json").SaveAsync(new List<Draw>
            {
                new() { JobId = job.Id, JobLabel = job.DisplayLine, Label = "Deposit", Amount = 5000m, TriggerNote = "At signing", InvoicedDate = DateTimeOffset.Now.AddDays(-28), PaidDate = DateTimeOffset.Now.AddDays(-27) },
                new() { JobId = job.Id, JobLabel = job.DisplayLine, Label = "Cabinet delivery draw", Amount = 8000m, TriggerNote = "On cabinet delivery", DueDate = DateTimeOffset.Now.AddDays(-3) },
            });
        }
        await folder.CreateFileAsync("remodel-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
