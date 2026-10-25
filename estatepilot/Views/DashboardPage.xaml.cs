using EstatePilot.Models;
using EstatePilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EstatePilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await new EstateSettingsStore().LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.ManagerName))
            TitleText.Text = $"{settings.ManagerName} - all properties";

        var properties = await new JsonListStore<Property>("properties.json").LoadAsync();
        var clocks = await new JsonListStore<ServiceClock>("clocks.json").LoadAsync();
        var vendors = await new JsonListStore<Vendor>("vendors.json").LoadAsync();
        var runs = await new JsonListStore<ChecklistRun>("checklistruns.json").LoadAsync();
        var staff = await new JsonListStore<StaffMember>("staff.json").LoadAsync();

        var lines = new List<string>();
        foreach (var c in clocks.OrderBy(c => c.NextDueDate).Take(10))
            lines.Add($"{(c.DaysUntil < 0 ? "OVERDUE" : c.DaysUntil <= 30 ? "DUE SOON" : "ok")} - {c.DisplayLine}");
        foreach (var s in staff.Where(s => s.DocDays is < 45).OrderBy(s => s.DocExpiry))
            lines.Add($"STAFF DOCUMENT CLOCK: {s.DisplayLine}");
        ClockText.Text = lines.Count == 0
            ? $"{properties.Count} propert(ies), no clocks recorded yet."
            : string.Join("\n", lines);

        var vlines = new List<string>();
        foreach (var v in vendors.Where(v => !v.InsuredNow || (v.CoiExpiry is { } ce && (ce.Date - DateTimeOffset.Now.Date).Days <= 30)).OrderBy(v => v.CoiExpiry))
            vlines.Add($"COI ATTENTION: {v.DisplayLine}");
        foreach (var r in runs.OrderByDescending(r => r.Date).Take(3))
            vlines.Add($"Checklist proof: {r.DisplayLine}");
        VendorText.Text = vlines.Count == 0 ? "All vendor COIs current beyond 30 days." : string.Join("\n", vlines);
    }
}
