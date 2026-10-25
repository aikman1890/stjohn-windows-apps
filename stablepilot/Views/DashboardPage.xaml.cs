using StablePilot.Models;
using StablePilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace StablePilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await new StableSettingsStore().LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.BarnName))
            TitleText.Text = $"{settings.BarnName} - dashboard";

        var horses = await new JsonListStore<Horse>("horses.json").LoadAsync();
        var clocks = await new JsonListStore<CareClock>("clocks.json").LoadAsync();
        var health = await new JsonListStore<HealthEntry>("health.json").LoadAsync();
        var month = DateTimeOffset.Now.ToString("yyyy-MM");
        var sessions = await new JsonListStore<SessionEntry>("sessions.json").LoadAsync();

        ClockText.Text = clocks.Count == 0 ? "No care clocks yet."
            : string.Join("\n", clocks.OrderBy(c => c.NextDueDate).Take(12).Select(c =>
                $"{(c.DaysUntil < 0 ? "OVERDUE" : c.DaysUntil <= c.OrangeBand ? "DUE SOON" : "ok")} - {c.DisplayLine}"));

        GlanceText.Text = $"{horses.Count} horse(s) boarded for {horses.Select(h => h.OwnerLabel).Distinct().Count()} owner(s).\n" +
            $"Board booked monthly: {string.Join(", ", horses.GroupBy(h => h.Currency).Select(g => $"{g.Sum(h => h.BoardRateMonthly):0.00} {g.Key}"))}.\n" +
            $"Health entries in the last 7 days: {health.Count(h => (DateTimeOffset.Now - h.Date).TotalDays <= 7)} - training sessions this month: {sessions.Count(s => s.Date.ToString("yyyy-MM") == month)}.";
    }
}
