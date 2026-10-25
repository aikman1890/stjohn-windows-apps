using YachtPilot.Models;
using YachtPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace YachtPilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await new YachtSettingsStore().LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.CompanyName))
            TitleText.Text = $"{settings.CompanyName} - fleet dashboard";

        var vessels = await new JsonListStore<Vessel>("vessels.json").LoadAsync();
        var crew = await new JsonListStore<CrewMember>("crew.json").LoadAsync();
        var certs = await new JsonListStore<Certificate>("certificates.json").LoadAsync();
        var maintenance = await new JsonListStore<MaintenanceItem>("maintenance.json").LoadAsync();
        var bookings = await new JsonListStore<Booking>("bookings.json").LoadAsync();
        var expenses = await new JsonListStore<Expense>("expenses.json").LoadAsync();
        var today = DateTimeOffset.Now.Date;

        var rows = vessels.Select(v =>
        {
            var blockers = Readiness.Blockers(v, crew, certs, maintenance);
            var soonCerts = certs.Count(c => crew.Any(cm => cm.Id == c.CrewId && cm.Active && cm.VesselId == v.Id) && !c.Expired && c.DaysUntil <= 30);
            var upcoming = bookings.Where(b => b.VesselId == v.Id && b.Status == "Booked" && b.EndDate.Date >= today)
                .OrderBy(b => b.StartDate).FirstOrDefault();
            var pressure = blockers.Count * 10 + soonCerts;
            var ready = blockers.Count == 0
                ? "CHARTER-READY"
                : v.ReadyOverrideDate is not null
                    ? $"NOT READY - overridden {v.ReadyOverrideDate:MMM d} with note on file"
                    : "NOT READY";
            var line = $"{v.Name} ({v.Flag}) - {ready}\n" +
                (blockers.Count > 0 ? "   " + string.Join("\n   ", blockers) + "\n" : string.Empty) +
                $"   Certs expiring within 30 days: {soonCerts}. " +
                (upcoming is null ? "No upcoming bookings." : $"Next: {upcoming.Kind} {upcoming.StartDate:MMM d} - {upcoming.EndDate:MMM d}.");
            return (Pressure: pressure, Line: line);
        }).OrderByDescending(r => r.Pressure).ToList();

        FleetList.ItemsSource = rows.Select(r => r.Line).ToList();
        var monthExpenses = expenses.Where(x => x.Date.ToString("yyyy-MM") == DateTimeOffset.Now.ToString("yyyy-MM")).ToList();
        var totals = monthExpenses.GroupBy(x => x.Currency)
            .Select(g => $"{g.Sum(x => x.Amount):0.00} {g.Key}");
        GlanceText.Text = $"{vessels.Count} vessel(s) under management - {crew.Count(c => c.Active)} active crew - " +
            $"{bookings.Count(b => b.Status == "Booked" && b.EndDate.Date >= today)} upcoming booking(s).\n" +
            $"Expenses logged this month: {(monthExpenses.Count == 0 ? "none yet" : string.Join(", ", totals))}.";
    }
}
