using HuntPilot.Models;
using HuntPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HuntPilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await new HuntSettingsStore().LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.RanchName))
            TitleText.Text = $"{settings.RanchName} - follow-up list";
        var norm = settings.StageNormDays <= 0 ? 30 : settings.StageNormDays;

        var trophies = await new JsonListStore<TrophyCase>("trophies.json").LoadAsync();
        var hunts = await new JsonListStore<HuntBooking>("hunts.json").LoadAsync();

        var calls = new List<string>();
        foreach (var t in trophies.Where(t => !t.Closed).OrderByDescending(t => t.DaysInStage))
        {
            if (t.DaysInStage > norm)
                calls.Add($"{(t.DaysInStage > norm * 2 ? "RED" : "ORANGE")} - {t.SpeciesLabel} for {t.HunterLabel}: {t.DaysInStage} days at '{t.Stage}' (norm {norm}) - call {(t.Stage.Contains("taxidermist") || t.Stage.Contains("Mount") ? t.TaxidermistName : t.ProcessorName)}.");
            else if (t.Stage == "Mount complete")
                calls.Add($"UPDATE - {t.SpeciesLabel} for {t.HunterLabel}: mount complete - arrange crating/shipping and tell the client.");
        }
        CallsText.Text = calls.Count == 0 ? "No trophy calls today - every open case is inside its norm."
            : string.Join("\n", calls);

        var open = hunts.Where(h => h.Status != "Cancelled" && h.Status != "Completed").ToList();
        var balances = hunts.Where(h => h.BalanceDue > 0 && h.Status != "Cancelled")
            .OrderByDescending(h => h.BalanceDue).Take(6)
            .Select(h => $"{h.HunterLabel}: balance {h.BalanceDue:0.00} {h.Currency} ({h.Status}, hunt {h.StartDate:MMM d})");
        var inCamp = hunts.Where(h => h.Status != "Cancelled" && h.StartDate.Date <= DateTimeOffset.Now.Date && h.EndDate.Date >= DateTimeOffset.Now.Date)
            .Select(h => $"In camp now: {h.HunterLabel} - {h.SpeciesTargeted} - guide {h.GuideLabel} - room {h.RoomNote}");
        MoneyText.Text = string.Join("\n",
            new[] { $"Open hunts: {open.Count}. Trophies outstanding: {trophies.Count(t => !t.Closed)}." }
                .Concat(balances).Concat(inCamp));
    }
}
