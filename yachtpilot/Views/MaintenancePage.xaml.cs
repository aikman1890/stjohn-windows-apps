using System.Globalization;
using YachtPilot.Models;
using YachtPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace YachtPilot.Views;

public sealed partial class MaintenancePage : Page
{
    private readonly JsonListStore<MaintenanceItem> _store = new("maintenance.json");
    private readonly JsonListStore<Vessel> _vesselStore = new("vessels.json");
    private List<MaintenanceItem> _items = new();
    private List<Vessel> _vessels = new();
    private MaintenanceItem? _selected;

    public MaintenancePage()
    {
        this.InitializeComponent();
        BasisBox.ItemsSource = new[] { "Date", "Engine hours" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _vessels = (await _vesselStore.LoadAsync()).OrderBy(v => v.Name).ToList();
        VesselBox.ItemsSource = _vessels.Select(v => v.Name).ToList();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _vessels = (await _vesselStore.LoadAsync()).OrderBy(v => v.Name).ToList();
        _items = (await _store.LoadAsync()).OrderBy(i => i.VesselLabel).ThenBy(i => i.Title).ToList();
        ItemList.ItemsSource = _items;
    }

    private void ItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemList.SelectedItem as MaintenanceItem;
        if (_selected is null) return;
        VesselBox.SelectedItem = _selected.VesselLabel;
        TitleBox.Text = _selected.Title;
        BasisBox.SelectedItem = _selected.Basis;
        NextDatePicker.Date = _selected.NextDueDate;
        NextHoursBox.Text = _selected.NextDueHours?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        NoteBox.Text = _selected.Note;
        DoneDatePicker.Date = DateTimeOffset.Now;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (VesselBox.SelectedIndex < 0) { ErrorText.Text = "Pick the vessel."; return; }
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) { ErrorText.Text = "Name the item."; return; }
        var item = _selected ?? new MaintenanceItem();
        var v = _vessels[VesselBox.SelectedIndex];
        item.VesselId = v.Id; item.VesselLabel = v.Name;
        item.Title = TitleBox.Text.Trim();
        item.Basis = BasisBox.SelectedItem as string ?? item.Basis;
        item.NextDueDate = NextDatePicker.Date;
        item.NextDueHours = decimal.TryParse(NextHoursBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var nh) ? nh : null;
        item.Note = NoteBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        await ReloadAsync();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemList.SelectedItem = null;
        TitleBox.Text = NoteBox.Text = NextHoursBox.Text = string.Empty;
        BasisBox.SelectedItem = "Date";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.Remove(_selected);
        await _store.SaveAsync(_items);
        _selected = null;
        await ReloadAsync();
    }

    private async void DoneButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the item first."; return; }
        var doneDate = DoneDatePicker.Date ?? DateTimeOffset.Now;
        _selected.LastDoneDate = doneDate;
        var vessel = _vessels.FirstOrDefault(v => v.Id == _selected.VesselId);
        if (_selected.Basis == "Engine hours")
        {
            _selected.LastDoneHours = vessel?.CurrentEngineHours;
            if (decimal.TryParse(DoneNextHoursBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var nh))
                _selected.NextDueHours = nh;
        }
        else
        {
            if (DateTimeOffset.TryParse(DoneNextDateBox.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var nd))
                _selected.NextDueDate = nd;
        }
        await _store.SaveAsync(_items);
        var activity = await new JsonListStore<ActivityEntry>("activity.json").LoadAsync();
        activity.Add(new ActivityEntry
        {
            VesselId = _selected.VesselId, VesselLabel = _selected.VesselLabel,
            Kind = "Service completed", Detail = _selected.Title,
            MethodNote = $"Completed {doneDate:MMM d, yyyy}", Date = DateTimeOffset.Now,
        });
        await new JsonListStore<ActivityEntry>("activity.json").SaveAsync(activity);
        await ReloadAsync();
    }
}
