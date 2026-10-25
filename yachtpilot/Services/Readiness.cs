using YachtPilot.Models;

namespace YachtPilot.Services;

public static class Readiness
{
    /// <summary>
    /// Charter-ready gate: a vessel is NOT ready while any assigned crew
    /// certificate is expired or any maintenance item is overdue. The
    /// manager can override only with a dated note on the vessel record.
    /// </summary>
    public static List<string> Blockers(Vessel vessel, List<CrewMember> crew, List<Certificate> certs, List<MaintenanceItem> maintenance)
    {
        var blockers = new List<string>();
        var assignedIds = crew.Where(c => c.Active && c.VesselId == vessel.Id).Select(c => c.Id).ToHashSet();
        foreach (var cert in certs.Where(c => assignedIds.Contains(c.CrewId) && c.Expired))
            blockers.Add($"EXPIRED CERTIFICATE: {cert.CrewLabel} - {cert.Type} (expired {-cert.DaysUntil} days ago).");
        foreach (var item in maintenance.Where(m => m.VesselId == vessel.Id && m.IsOverdue(vessel.CurrentEngineHours)))
            blockers.Add($"MAINTENANCE OVERDUE: {item.Title}.");
        if (vessel.InsuranceExpiry is { } ins && ins.Date < DateTimeOffset.Now.Date)
            blockers.Add("INSURANCE EXPIRED on the vessel record.");
        if (vessel.RegistrationExpiry is { } reg && reg.Date < DateTimeOffset.Now.Date)
            blockers.Add("REGISTRATION EXPIRED on the vessel record.");
        return blockers;
    }

    public static bool IsReady(Vessel vessel, List<CrewMember> crew, List<Certificate> certs, List<MaintenanceItem> maintenance) =>
        Blockers(vessel, crew, certs, maintenance).Count == 0;
}
