using System.Globalization;
using RallyPilot.Models;
using RallyPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RallyPilot.Views;

public sealed partial class PackagesPage : Page
{
    private readonly JsonListStore<EventPackage> _store = new("packages.json");
    private readonly JsonListStore<AddOnDef> _addStore = new("addons.json");
    private List<EventPackage> _packages = new();
    private EventPackage? _selected;

    public PackagesPage()
    {
        this.InitializeComponent();
        AddKindBox.ItemsSource = new[] { "Per head", "Per group", "Flat" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _packages = (await _store.LoadAsync()).OrderBy(p => p.Name).ToList();
        PackageList.ItemsSource = _packages;
        AddOnList.ItemsSource = await _addStore.LoadAsync();
        if (_packages.Count > 0) PackageList.SelectedIndex = 0;
    }

    private void PackageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = PackageList.SelectedItem as EventPackage;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        ComponentsBox.Text = _selected.Components;
        PerHeadBox.Text = _selected.PerHeadPrice.ToString(CultureInfo.InvariantCulture);
        PerGroupBox.Text = _selected.PerGroupPrice.ToString(CultureInfo.InvariantCulture);
        MinBox.Text = _selected.MinHeads.ToString(CultureInfo.InvariantCulture);
        StaffingBox.Text = _selected.StaffingNote;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "Name the package."; return; }
        var p = _selected ?? new EventPackage();
        p.Name = NameBox.Text.Trim();
        p.Components = ComponentsBox.Text.Trim();
        if (decimal.TryParse(PerHeadBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var ph)) p.PerHeadPrice = ph;
        if (decimal.TryParse(PerGroupBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var pg)) p.PerGroupPrice = pg;
        if (int.TryParse(MinBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var min)) p.MinHeads = min;
        p.StaffingNote = StaffingBox.Text.Trim();
        if (_selected is null) _packages.Add(p);
        await _store.SaveAsync(_packages);
        PackageList.ItemsSource = _packages;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        PackageList.SelectedItem = null;
        NameBox.Text = ComponentsBox.Text = StaffingBox.Text = string.Empty;
        PerHeadBox.Text = PerGroupBox.Text = MinBox.Text = "0";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _packages.Remove(_selected);
        await _store.SaveAsync(_packages);
        _selected = null;
        PackageList.ItemsSource = _packages;
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AddNameBox.Text)) { ErrorText.Text = "Name the add-on."; return; }
        var adds = await _addStore.LoadAsync();
        adds.Add(new AddOnDef
        {
            Name = AddNameBox.Text.Trim(),
            Price = decimal.TryParse(AddPriceBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ? price : 0m,
            PricingKind = AddKindBox.SelectedItem as string ?? "Per head",
        });
        await _addStore.SaveAsync(adds);
        AddOnList.ItemsSource = adds;
        AddNameBox.Text = AddPriceBox.Text = string.Empty;
    }
}
