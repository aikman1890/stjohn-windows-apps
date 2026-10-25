using System.Globalization;
using HuntPilot.Models;
using HuntPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HuntPilot.Views;

public sealed partial class SpeciesPage : Page
{
    private readonly JsonListStore<SpeciesEntry> _store = new("species.json");
    private List<SpeciesEntry> _species = new();
    private SpeciesEntry? _selected;

    public SpeciesPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _species = (await _store.LoadAsync()).OrderBy(s => s.Name).ToList();
        SpeciesList.ItemsSource = _species;
        if (_species.Count > 0) SpeciesList.SelectedIndex = 0;
    }

    private void SpeciesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = SpeciesList.SelectedItem as SpeciesEntry;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        FeeBox.Text = _selected.TrophyFee.ToString(CultureInfo.InvariantCulture);
        SeasonBox.Text = _selected.SeasonNote;
        MethodsBox.Text = _selected.MethodsNote;
        HerdBox.Text = _selected.HerdNote;
        PackageBox.Text = _selected.PackageNote;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "Name the species."; return; }
        var s = _selected ?? new SpeciesEntry();
        s.Name = NameBox.Text.Trim();
        if (decimal.TryParse(FeeBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var fee)) s.TrophyFee = fee;
        s.SeasonNote = SeasonBox.Text.Trim();
        s.MethodsNote = MethodsBox.Text.Trim();
        s.HerdNote = HerdBox.Text.Trim();
        s.PackageNote = PackageBox.Text.Trim();
        if (_selected is null) _species.Add(s);
        await _store.SaveAsync(_species);
        _species = _species.OrderBy(x => x.Name).ToList();
        SpeciesList.ItemsSource = _species;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        SpeciesList.SelectedItem = null;
        NameBox.Text = FeeBox.Text = SeasonBox.Text = MethodsBox.Text = HerdBox.Text = PackageBox.Text = string.Empty;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _species.Remove(_selected);
        await _store.SaveAsync(_species);
        _selected = null;
        SpeciesList.ItemsSource = _species;
    }
}
