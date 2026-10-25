using WheelPilot.Models;

namespace WheelPilot.Services;

public static class BalanceCalc
{
    /// <summary>
    /// Hours balance for a student: purchased - completed lessons + adjustments.
    /// Every movement is a record; the balance is derived, never stored stale.
    /// </summary>
    public static decimal HoursRemaining(string studentId, List<Purchase> purchases, List<Lesson> lessons, List<Adjustment> adjustments)
    {
        var bought = purchases.Where(p => p.StudentId == studentId).Sum(p => p.Hours);
        var used = lessons.Where(l => l.StudentId == studentId && l.Status == "Completed").Sum(l => l.DurationHours);
        var adj = adjustments.Where(a => a.StudentId == studentId).Sum(a => a.HoursDelta);
        return bought - used + adj;
    }

    public static decimal MoneyOwed(string studentId, List<Purchase> purchases) =>
        purchases.Where(p => p.StudentId == studentId).Sum(p => p.Owed);
}
