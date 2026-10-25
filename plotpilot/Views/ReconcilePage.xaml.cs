using PlotPilot.Models;
using PlotPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlotPilot.Views;

public sealed partial class ReconcilePage : Page
{
    private readonly JsonListStore<PlotSpace> _spaceStore = new("spaces.json");
    private readonly JsonListStore<BurialRecord> _burialStore = new("burials.json");
    private readonly JsonListStore<ChainEntry> _chainStore = new("chain.json");

    public ReconcilePage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var conflicts = Reconciler.FindConflicts(
            await _spaceStore.LoadAsync(),
            await _burialStore.LoadAsync(),
            await _chainStore.LoadAsync());
        CountText.Text = conflicts.Count == 0
            ? "No conflicts found - plat, notebooks, and register agree."
            : $"{conflicts.Count} item(s) for the board:";
        ConflictList.ItemsSource = conflicts;
    }
}
