using PlotPilot.Models;
using PlotPilot.Services;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI;

namespace PlotPilot.Views;

public sealed partial class MapPage : Page
{
    private readonly JsonListStore<PlotSpace> _spaceStore = new("spaces.json");
    private readonly JsonListStore<BurialRecord> _burialStore = new("burials.json");
    private readonly PlotSettingsStore _settingsStore = new();
    private List<PlotSpace> _spaces = new();
    private List<BurialRecord> _burials = new();

    public MapPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        _spaces = await _spaceStore.LoadAsync();
        _burials = await _burialStore.LoadAsync();
        var sections = settings.Sections.Concat(_spaces.Select(s => s.Section))
            .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        SectionPicker.ItemsSource = sections;
        if (sections.Count > 0) SectionPicker.SelectedIndex = 0;
        Render();
    }

    private void SectionPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => Render();

    private static Brush ChipBrush(string status) => status switch
    {
        "Available" => new SolidColorBrush(Color.FromArgb(255, 60, 160, 80)),
        "Sold" => new SolidColorBrush(Color.FromArgb(255, 90, 130, 200)),
        "Reserved" => new SolidColorBrush(Color.FromArgb(255, 220, 140, 30)),
        "Occupied" => new SolidColorBrush(Color.FromArgb(255, 120, 120, 120)),
        _ => new SolidColorBrush(Color.FromArgb(255, 170, 90, 160)),
    };

    private void Render()
    {
        GridPanel.Children.Clear();
        var section = SectionPicker.SelectedItem as string;
        if (section is null) return;
        var mine = _spaces.Where(s => s.Section == section).ToList();
        foreach (var lotGroup in mine.GroupBy(s => s.Lot).OrderBy(g => g.Key))
        {
            GridPanel.Children.Add(new TextBlock
            {
                Text = lotGroup.Key,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 2),
            });
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            foreach (var space in lotGroup.OrderBy(s => s.Space))
            {
                var captured = space;
                var button = new Button
                {
                    Content = space.Space,
                    Background = ChipBrush(space.Status),
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                    Width = 52,
                    Height = 40,
                    Tag = captured,
                };
                button.Click += Chip_Click;
                row.Children.Add(button);
            }
            GridPanel.Children.Add(new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollMode = ScrollMode.Enabled,
                Content = row,
            });
        }
        if (mine.Count == 0)
            GridPanel.Children.Add(new TextBlock { Text = "No spaces in this section yet - add them in the Register." });
    }

    private void Chip_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not PlotSpace space) return;
        var burialsHere = _burials.Where(b => b.SpaceId == space.Id).ToList();
        DetailText.Text =
            $"{space.Address} - {space.Status}" +
            (string.IsNullOrWhiteSpace(space.OwnerName) ? string.Empty : $"{Environment.NewLine}Owner of record: {space.OwnerName}") +
            (string.IsNullOrWhiteSpace(space.DeedRef) ? string.Empty : $"{Environment.NewLine}Deed: {space.DeedRef}") +
            (space.PurchaseDate is { } p ? $"{Environment.NewLine}Purchased {p:MMM d, yyyy} at {space.PurchasePrice:C} (care {space.CareContribution:C})" : string.Empty) +
            (burialsHere.Count == 0
                ? $"{Environment.NewLine}No burial records for this space."
                : string.Concat(burialsHere.Select(b => $"{Environment.NewLine}Buried: {b.DecedentName} - interred {b.IntermentDate:MMM d, yyyy} - {b.MarkerNotes}"))) +
            (string.IsNullOrWhiteSpace(space.Source) ? string.Empty : $"{Environment.NewLine}Source: {space.Source}") +
            (string.IsNullOrWhiteSpace(space.Notes) ? string.Empty : $"{Environment.NewLine}Notes: {space.Notes}");
    }
}
