using FarewellPilot.Models;

namespace FarewellPilot.Services;

public static class CaseMath
{
    public static decimal ContractTotal(List<ContractLine> lines, string caseId) =>
        lines.Where(l => l.CaseId == caseId).Sum(l => l.Amount);

    public static decimal PaidTotal(List<Payment> payments, List<InsuranceAssignment> assignments, string caseId) =>
        payments.Where(p => p.CaseId == caseId).Sum(p => p.Amount) +
        assignments.Where(a => a.CaseId == caseId && a.Status == "Paid").Sum(a => a.Amount);

    public static decimal Balance(List<ContractLine> lines, List<Payment> payments,
        List<InsuranceAssignment> assignments, string caseId) =>
        ContractTotal(lines, caseId) - PaidTotal(payments, assignments, caseId);

    public static string AgingBucket(FuneralCase c)
    {
        var days = (DateTimeOffset.Now.Date - c.FirstCallDate.Date).Days;
        return days <= 30 ? "0-30 days" : days <= 60 ? "31-60 days" : days <= 90 ? "61-90 days" : "90+ days";
    }

    /// <summary>Builds the checklist for a new case from the settings templates for its type.</summary>
    public static List<ChecklistItem> BuildChecklist(FarewellSettings settings, string serviceType)
    {
        var items = settings.BaseChecklist.Select(l => new ChecklistItem { Label = l }).ToList();
        if (serviceType == "Burial" || serviceType == "Graveside")
            items.AddRange(settings.BurialExtras.Select(l => new ChecklistItem { Label = l }));
        if (serviceType == "Cremation")
            items.AddRange(settings.CremationExtras.Select(l => new ChecklistItem { Label = l }));
        return items;
    }
}
