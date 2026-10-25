using AgreementPilot.Models;
using AgreementPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AgreementPilot.Views;

public sealed partial class WatchPage : Page
{
    private readonly JsonListStore<Equipment> _equipmentStore = new("equipment.json");
    private readonly JsonListStore<Visit> _visitStore = new("visits.json");
    private readonly TradeSettingsStore _settingsStore = new();

    public WatchPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var equipment = await _equipmentStore.LoadAsync();
        var visits = await _visitStore.LoadAsync();

        var watched = equipment
            .Where(eq => eq.AgeYears is { } age && age >= settings.WatchAgeYears)
            .OrderByDescending(eq => eq.AgeYears).ToList();

        SubText.Text = $"Units at or past the watch age ({settings.WatchAgeYears} years, set in Settings): {watched.Count}. " +
                       "Your shop installed or services these - the replacement conversation starts here, before the breakdown sends the customer shopping.";

        WatchList.ItemsSource = watched.Select(eq =>
        {
            var mine = visits.Where(v => v.EquipmentId == eq.Id).OrderByDescending(v => v.Date).ToList();
            var last = mine.FirstOrDefault();
            var recs = mine.Count(v => !string.IsNullOrWhiteSpace(v.Recommendation));
            return $"{eq.FullLabel} - {eq.LocationNote} - {mine.Count} visit(s) on record" +
                   (last is null ? string.Empty : $", last {last.Date:MMM d, yyyy}: {last.WorkPerformed}") +
                   (recs > 0 ? $" - {recs} recommendation(s) logged" : string.Empty);
        }).ToList();
    }
}
