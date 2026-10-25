using RemodelPilot.Models;

namespace RemodelPilot.Services;

/// <summary>One place for the job-money math so every page agrees.</summary>
public static class JobMath
{
    public static decimal ContractPrice(Job job, List<ChangeOrder> cos) =>
        job.BaseContractPrice + cos.Where(c => c.JobId == job.Id && c.Status == "Approved").Sum(c => c.Price);

    public static decimal ActualTotal(List<ActualCost> actuals, string jobId) =>
        actuals.Where(a => a.JobId == jobId).Sum(a => a.Amount);

    public static Dictionary<string, decimal> BudgetByCategory(EstimateVersion? winning) =>
        winning is null
            ? new Dictionary<string, decimal>()
            : winning.Lines.GroupBy(l => l.Category)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.Cost), StringComparer.OrdinalIgnoreCase);

    public static Dictionary<string, decimal> ActualByCategory(List<ActualCost> actuals, string jobId) =>
        actuals.Where(a => a.JobId == jobId)
            .GroupBy(a => a.Category)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Amount), StringComparer.OrdinalIgnoreCase);
}
