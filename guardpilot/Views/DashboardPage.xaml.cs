using GuardPilot.Models;
using GuardPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GuardPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<Shift> _shiftStore = new("shifts.json");
    private readonly JsonListStore<Guard> _guardStore = new("guards.json");
    private readonly JsonListStore<Incident> _incidentStore = new("incidents.json");
    private readonly GuardSettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var shifts = await _shiftStore.LoadAsync();
        var guards = await _guardStore.LoadAsync();
        var incidents = await _incidentStore.LoadAsync();

        CompanyText.Text = string.IsNullOrWhiteSpace(settings.CompanyName)
            ? "Set the company in Settings. The office system of record - posts, orders, rosters, incidents, billing hours."
            : settings.CompanyName;

        var today = DateTimeOffset.Now.Date;
        var monday = today.AddDays(-(((int)DateTimeOffset.Now.DayOfWeek + 6) % 7));
        var week = shifts.Where(s => s.Date.Date >= monday && s.Date.Date < monday.AddDays(7)).ToList();
        WeekText.Text =
            $"Shifts this week: {week.Count} - scheduled hours {week.Sum(s => s.ScheduledHours):0.0}" +
            $"{Environment.NewLine}Unfilled: {week.Count(s => s.Unfilled)} - actual hours entered: {week.Where(s => s.ActualHours is not null).Sum(s => s.ActualHours ?? 0m):0.0}";

        var expiring = guards.Where(g => g.Status == "Active" && g.LicenseDays is { } d && d <= 30).ToList();
        RiskText.Text =
            $"Licences expiring in 30 days or already past: {expiring.Count}" +
            $"{Environment.NewLine}Open incident follow-ups: {incidents.Count(i => !i.FollowUpDone && !string.IsNullOrWhiteSpace(i.FollowUp))} - overdue: {incidents.Count(i => i.FollowUpOverdue)}";

        ProblemList.ItemsSource = ScheduleCheck.FindProblems(week, guards, settings.OvertimeWeeklyThreshold);
        TopList.ItemsSource = week.GroupBy(s => s.SiteLabel).OrderByDescending(g => g.Sum(s => s.ScheduledHours))
            .Select(g => $"{g.Key}: {g.Sum(s => s.ScheduledHours):0.0}h scheduled - {g.Sum(s => s.ScheduledHours * s.BillRatePerHour):C} at contract rates")
            .ToList();
    }
}
