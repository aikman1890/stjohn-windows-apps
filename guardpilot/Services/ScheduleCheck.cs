using GuardPilot.Models;

namespace GuardPilot.Services;

public static class ScheduleCheck
{
    /// <summary>Shifts on the same date whose hour ranges overlap for the same guard.</summary>
    public static List<string> FindProblems(List<Shift> shifts, List<Guard> guards, decimal overtimeThreshold)
    {
        var problems = new List<string>();
        foreach (var s in shifts.Where(s => s.Unfilled))
            problems.Add($"UNFILLED: {s.DisplayLine}");

        var byGuardDay = shifts.Where(s => !s.Unfilled)
            .GroupBy(s => (s.GuardId, s.Date.Date));
        foreach (var group in byGuardDay)
        {
            var list = group.OrderBy(s => s.StartHour).ToList();
            for (var i = 0; i < list.Count; i++)
            {
                for (var j = i + 1; j < list.Count; j++)
                {
                    if (list[i].StartHour < list[j].EndHour && list[j].StartHour < list[i].EndHour)
                        problems.Add($"DOUBLE-BOOKED: {list[i].GuardLabel} on {list[i].Date:MMM d} - {list[i].PostLabel} ({list[i].StartHour:0.0}-{list[i].EndHour:0.0}h) overlaps {list[j].PostLabel} ({list[j].StartHour:0.0}-{list[j].EndHour:0.0}h).");
                }
            }
        }

        foreach (var s in shifts.Where(s => !s.Unfilled))
        {
            var guard = guards.FirstOrDefault(g => g.Id == s.GuardId);
            if (guard?.LicenseExpiry is { } exp && exp.Date < s.Date.Date)
                problems.Add($"LICENCE: {guard.Name} works {s.PostLabel} on {s.Date:MMM d} but their recorded licence expired {exp:MMM d, yyyy}.");
        }

        // Overtime accumulation per guard per ISO-ish week (Monday start).
        foreach (var group in shifts.Where(s => !s.Unfilled).GroupBy(s => s.GuardId))
        {
            var byWeek = group.GroupBy(s => s.Date.Date.AddDays(-(((int)s.Date.DayOfWeek + 6) % 7)));
            foreach (var week in byWeek)
            {
                var hours = week.Sum(s => s.ScheduledHours);
                if (hours > overtimeThreshold)
                    problems.Add($"OVERTIME: {week.First().GuardLabel} is scheduled {hours:0.0}h in the week of {week.Key:MMM d} (threshold {overtimeThreshold:0}h).");
            }
        }
        return problems;
    }
}
