using PlotPilot.Models;
using PlotPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlotPilot.Views;

public sealed partial class SearchPage : Page
{
    private readonly JsonListStore<PlotSpace> _spaceStore = new("spaces.json");
    private readonly JsonListStore<BurialRecord> _burialStore = new("burials.json");
    private List<PlotSpace> _spaces = new();
    private List<BurialRecord> _burials = new();

    public SearchPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _spaces = await _spaceStore.LoadAsync();
        _burials = await _burialStore.LoadAsync();
        NearPicker.ItemsSource = _spaces.OrderBy(s => s.Address).Select(s => s.Address).ToList();
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var q = QueryBox.Text.Trim();
        if (q.Length == 0) return;
        bool Hit(string s) => s.Contains(q, StringComparison.OrdinalIgnoreCase);
        var lines = new List<string>();
        lines.AddRange(_burials.Where(b => Hit(b.DecedentName))
            .Select(b => $"BURIAL: {b.DisplayLine}"));
        lines.AddRange(_spaces.Where(s => Hit(s.OwnerName) || Hit(s.Address))
            .Select(s => $"SPACE: {s.DisplayLine}"));
        if (lines.Count == 0) lines.Add("No matches in burials, owners, or addresses.");
        ResultsList.ItemsSource = lines;
    }

    private void NearPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NearPicker.SelectedItem is not string address) return;
        var anchor = _spaces.FirstOrDefault(s => s.Address == address);
        if (anchor is null) return;
        NearList.ItemsSource = _spaces
            .Where(s => s.Section == anchor.Section && s.Block == anchor.Block && s.Status == "Available")
            .OrderBy(s => s.Address).ToList();
    }
}
