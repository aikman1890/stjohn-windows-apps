using TaxPilot.Models;

namespace TaxPilot.Services;

public static class DeadlineCalc
{
    /// <summary>
    /// Due date from the preparer's own table: "Entity|MM-DD|Ext MM-DD", filing
    /// year = tax year + 1. Personal per-client dates win; extension dates apply
    /// once the engagement is marked extended.
    /// </summary>
    public static DateTimeOffset? ComputeDue(Engagement e, TaxSettings settings)
    {
        if (e.PersonalDueDate is { } personal) return personal;
        foreach (var line in settings.DeadlineTable)
        {
            var parts = line.Split('|');
            if (parts.Length < 3 || parts[0].Trim() != e.EntityType) continue;
            var mmdd = (e.Extended ? parts[2] : parts[1]).Trim().Split('-');
            if (mmdd.Length == 2 &&
                int.TryParse(mmdd[0], out var month) &&
                int.TryParse(mmdd[1], out var day))
            {
                try { return new DateTimeOffset(e.TaxYear + 1, month, day, 0, 0, 0, TimeSpan.Zero); }
                catch { return null; }
            }
        }
        return null;
    }

    public static void ApplyDueDates(List<Engagement> engagements, TaxSettings settings)
    {
        foreach (var e in engagements) e.ComputedDue = ComputeDue(e, settings);
    }
}
