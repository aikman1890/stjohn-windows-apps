using HealthPilot.Models;
using HealthPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HealthPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<Medication> _medStore = new("medications.json");
    private readonly JsonListStore<AppointmentItem> _apptStore = new("appointments.json");
    private readonly JsonListStore<LogEntry> _logStore = new("healthlog.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var meds = await _medStore.LoadAsync();
        var appts = await _apptStore.LoadAsync();
        var log = await _logStore.LoadAsync();

        MedText.Text = meds.Count.ToString();
        var upcoming = appts.Where(a => a.When >= DateTimeOffset.Now).OrderBy(a => a.When).ToList();
        ApptText.Text = upcoming.Count.ToString();
        LogText.Text = log.Count(l => l.Date >= DateTimeOffset.Now.AddDays(-30)).ToString();
        NextApptText.Text = upcoming.Count > 0
            ? $"Next appointment: {upcoming[0].DisplayLine}"
            : "Next appointment: none scheduled";
        RecentList.ItemsSource = log
            .OrderByDescending(l => l.Date)
            .Take(10)
            .Select(l => $"{l.DisplayLine} - {l.SubLine}")
            .ToList();
    }
}
