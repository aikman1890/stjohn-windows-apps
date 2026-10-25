using System.Globalization;
using HuntPilot.Models;
using HuntPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HuntPilot.Views;

public sealed partial class HarvestPage : Page
{
    private readonly JsonListStore<Harvest> _store = new("harvests.json");
    private readonly JsonListStore<HuntBooking> _huntStore = new("hunts.json");
    private readonly JsonListStore<SpeciesEntry> _speciesStore = new("species.json");
    private List<HuntBooking> _hunts = new();
    private List<SpeciesEntry> _species = new();

    public HarvestPage()
    {
        this.InitializeComponent();
        DressedBox.ItemsSource = new[] { "Yes", "Not yet" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _hunts = (await _huntStore.LoadAsync()).OrderByDescending(h => h.StartDate).ToList();
        HuntBox.ItemsSource = _hunts.Select(h => $"{h.StartDate:MMM d} - {h.HunterLabel} - {h.SpeciesTargeted}").ToList();
        _species = (await _speciesStore.LoadAsync()).OrderBy(s => s.Name).ToList();
        SpeciesBox.ItemsSource = _species.Select(s => s.Name).ToList();
        HarvestList.ItemsSource = (await _store.LoadAsync()).OrderByDescending(h => h.Date).ToList();
    }

    private void SpeciesBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SpeciesBox.SelectedIndex < 0 || SpeciesBox.SelectedIndex >= _species.Count) return;
        FeeBox.Text = _species[SpeciesBox.SelectedIndex].TrophyFee.ToString(CultureInfo.InvariantCulture);
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (HuntBox.SelectedIndex < 0) { ErrorText.Text = "Pick the hunt."; return; }
        if (SpeciesBox.SelectedIndex < 0) { ErrorText.Text = "Pick the species."; return; }
        var hunt = _hunts[HuntBox.SelectedIndex];
        var species = _species[SpeciesBox.SelectedIndex];
        var fee = decimal.TryParse(FeeBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var f) ? f : species.TrophyFee;
        var dressed = DressedBox.SelectedItem as string == "Yes";
        var harvest = new Harvest
        {
            HuntId = hunt.Id, HunterLabel = hunt.HunterLabel, SpeciesLabel = species.Name,
            Date = DateTimeOffset.Now, GuideLabel = hunt.GuideLabel,
            Measurements = MeasureBox.Text.Trim(), PhotoPath = PhotoBox.Text.Trim(),
            FieldDressed = dressed, FieldDressedDate = dressed ? DateTimeOffset.Now : null,
            TrophyFee = fee, Notes = NotesBox.Text.Trim(),
        };
        var harvests = await _store.LoadAsync();
        harvests.Add(harvest);
        await _store.SaveAsync(harvests);

        hunt.TrophyFees += fee;
        await _huntStore.SaveAsync(_hunts);

        var cases = await new JsonListStore<TrophyCase>("trophies.json").LoadAsync();
        cases.Add(new TrophyCase
        {
            HarvestId = harvest.Id, HunterLabel = hunt.HunterLabel, SpeciesLabel = species.Name,
            Stage = dressed ? "Field dressed" : "Field dressed",
            StageDate = DateTimeOffset.Now,
            Notes = dressed ? "Field dressing completed at harvest." : "Field dressing still to complete - case opened at harvest.",
        });
        await new JsonListStore<TrophyCase>("trophies.json").SaveAsync(cases);

        HarvestList.ItemsSource = harvests.OrderByDescending(h => h.Date).ToList();
        MeasureBox.Text = PhotoBox.Text = NotesBox.Text = string.Empty;
    }
}
