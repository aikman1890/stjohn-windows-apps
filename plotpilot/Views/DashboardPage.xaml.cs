using PlotPilot.Models;
using PlotPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlotPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<PlotSpace> _spaceStore = new("spaces.json");
    private readonly JsonListStore<BurialRecord> _burialStore = new("burials.json");
    private readonly JsonListStore<ChainEntry> _chainStore = new("chain.json");
    private readonly PlotSettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var spaces = await _spaceStore.LoadAsync();
        var burials = await _burialStore.LoadAsync();
        var chain = await _chainStore.LoadAsync();

        CemeteryText.Text = string.IsNullOrWhiteSpace(settings.CemeteryName)
            ? "Set the cemetery in Settings. One searchable record - who is buried where, who owns which plot, and what the old books say."
            : settings.CemeteryName;

        InventoryText.Text = string.Join(Environment.NewLine,
            spaces.GroupBy(s => s.Status).OrderByDescending(g => g.Count())
                .Select(g => $"{g.Key}: {g.Count()}")) +
            $"{Environment.NewLine}Total spaces on the register: {spaces.Count}";

        var conflicts = Reconciler.FindConflicts(spaces, burials, chain);
        HealthText.Text =
            $"Burial records: {burials.Count} - ownership chain entries: {chain.Count}" +
            $"{Environment.NewLine}Items for the board in Reconciliation: {conflicts.Count}" +
            $"{Environment.NewLine}Care contributions recorded: {spaces.Sum(s => s.CareContribution):C} (record only - the fund's books stay with the treasurer)";

        RecentList.ItemsSource = burials.OrderByDescending(b => b.IntermentDate).Take(8).ToList();
    }
}
