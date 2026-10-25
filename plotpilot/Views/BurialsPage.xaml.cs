using PlotPilot.Models;
using PlotPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlotPilot.Views;

public sealed partial class BurialsPage : Page
{
    private readonly JsonListStore<BurialRecord> _store = new("burials.json");
    private readonly JsonListStore<PlotSpace> _spaceStore = new("spaces.json");
    private List<BurialRecord> _items = new();
    private List<PlotSpace> _spaces = new();
    private List<PlotSpace> _ordered = new();
    private BurialRecord? _selected;

    public BurialsPage()
    {
        this.InitializeComponent();
        InterPicker.Date = DateTimeOffset.Now;
        BirthPicker.Date = DateTimeOffset.Now.AddYears(-70);
        DeathPicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _spaces = await _spaceStore.LoadAsync();
        _ordered = _spaces.OrderBy(s => s.Address).ToList();
        SpacePicker.ItemsSource = _ordered.Select(s => s.Address).ToList();
        if (_ordered.Count > 0) SpacePicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(b => b.IntermentDate).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as BurialRecord;
        if (_selected is null) return;
        var idx = _ordered.FindIndex(s => s.Id == _selected.SpaceId);
        if (idx >= 0) SpacePicker.SelectedIndex = idx;
        NameBox.Text = _selected.DecedentName;
        BirthCheck.IsChecked = _selected.BirthDate is not null;
        if (_selected.BirthDate is { } b) BirthPicker.Date = b;
        DeathCheck.IsChecked = _selected.DeathDate is not null;
        if (_selected.DeathDate is { } d) DeathPicker.Date = d;
        InterPicker.Date = _selected.IntermentDate;
        FuneralBox.Text = _selected.FuneralHome;
        MarkerBox.Text = _selected.MarkerNotes;
        VeteranCheck.IsChecked = _selected.Veteran;
        SourceBox.Text = _selected.Source;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (SpacePicker.SelectedIndex < 0 || SpacePicker.SelectedIndex >= _ordered.Count) return;
        var space = _ordered[SpacePicker.SelectedIndex];
        var item = _selected ?? new BurialRecord();
        item.SpaceId = space.Id;
        item.SpaceLabel = space.Address;
        item.DecedentName = NameBox.Text.Trim();
        item.BirthDate = BirthCheck.IsChecked == true ? BirthPicker.Date : null;
        item.DeathDate = DeathCheck.IsChecked == true ? DeathPicker.Date : null;
        item.IntermentDate = InterPicker.Date ?? DateTimeOffset.Now;
        item.FuneralHome = FuneralBox.Text.Trim();
        item.MarkerNotes = MarkerBox.Text.Trim();
        item.Veteran = VeteranCheck.IsChecked == true;
        item.Source = SourceBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        if (space.Status != "Occupied")
        {
            space.Status = "Occupied";
            await _spaceStore.SaveAsync(_spaces);
        }
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(b => b.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
    }

    private void ClearForm()
    {
        NameBox.Text = string.Empty;
        FuneralBox.Text = string.Empty;
        MarkerBox.Text = string.Empty;
        VeteranCheck.IsChecked = false;
        BirthCheck.IsChecked = false;
        DeathCheck.IsChecked = false;
        SourceBox.Text = string.Empty;
    }
}
