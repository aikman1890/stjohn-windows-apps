using AgreementPilot.Models;
using AgreementPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AgreementPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<Agreement> _agreementStore = new("agreements.json");
    private readonly JsonListStore<Equipment> _equipmentStore = new("equipment.json");
    private readonly JsonListStore<Visit> _visitStore = new("visits.json");
    private readonly TradeSettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var agreements = await _agreementStore.LoadAsync();
        var equipment = await _equipmentStore.LoadAsync();
        var visits = await _visitStore.LoadAsync();

        ShopText.Text = string.IsNullOrWhiteSpace(settings.ShopName)
            ? "Set the shop in Settings. The book: equipment per address, agreements with clocks, recommendations that don't evaporate."
            : settings.ShopName;

        var active = agreements.Where(a => a.Status is "Active" or "Renewal due").ToList();
        int In(int days) => active.Count(a => a.DaysToRenewal >= 0 && a.DaysToRenewal <= days);
        RenewalText.Text =
            $"Expiring in 30 days: {In(30)} - 60 days: {In(60)} - 90 days: {In(90)}" +
            $"{Environment.NewLine}Overdue now: {active.Count(a => a.DaysToRenewal < 0)}" +
            $"{Environment.NewLine}Active book: {active.Count} agreement(s), {active.Sum(a => a.Price):C}/yr";

        var owed = active.Sum(a => a.VisitsOwed);
        var watch = equipment.Count(eq => eq.AgeYears is { } age && age >= settings.WatchAgeYears);
        WorkText.Text =
            $"Tune-up visits owed: {owed} across {active.Count(a => a.VisitsOwed > 0)} agreement(s)" +
            $"{Environment.NewLine}Units on the replacement watch ({settings.WatchAgeYears}+ years): {watch}";

        DeclinedList.ItemsSource = visits
            .Where(v => v.RecResponse == "Declined" && !v.FollowUpDone)
            .OrderBy(v => v.Date)
            .Select(v => $"{v.CustomerLabel} - {v.Recommendation} ({v.RecAmount:C}) - declined {v.Date:MMM d, yyyy} on {v.EquipmentLabel}")
            .ToList();

        LapsedList.ItemsSource = agreements.Where(a => a.Status == "Lapsed")
            .OrderBy(a => a.RenewalDate).ToList();
    }
}
