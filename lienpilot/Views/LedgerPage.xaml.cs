using LienPilot.Models;
using LienPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LienPilot.Views;

public sealed partial class LedgerPage : Page
{
    private readonly JsonListStore<LienLedgerEntry> _store = new("ledger.json");
    private readonly JsonListStore<StorageUnit> _unitStore = new("units.json");
    private readonly JsonListStore<StateProfile> _profileStore = new("profiles.json");
    private readonly FacilitySettingsStore _settingsStore = new();
    private List<LienLedgerEntry> _items = new();
    private List<StorageUnit> _units = new();
    private LienLedgerEntry? _selected;

    public LedgerPage()
    {
        this.InitializeComponent();
        DatePicker.Date = DateTimeOffset.Now;
        MethodCombo.ItemsSource = new[] { "Verified mail", "Certified mail", "Email", "Hand delivery", "Publication", "Posting", "Other" };
        MethodCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _units = await _unitStore.LoadAsync();
        UnitPicker.ItemsSource = _units.Select(u => u.DisplayLine).ToList();
        if (_units.Count > 0) UnitPicker.SelectedIndex = 0;
        var settings = await _settingsStore.LoadAsync();
        var profiles = await _profileStore.LoadAsync();
        var profile = profiles.FirstOrDefault(p => p.Id == settings.ActiveProfileId) ?? profiles.FirstOrDefault();
        var steps = profile?.RequiredSteps is { Count: > 0 } rs
            ? rs
            : new List<string> { "Default notice", "Lien notice", "Advertisement week 1", "Advertisement week 2" };
        StepCombo.ItemsSource = steps.Concat(new[] { "Payment received", "Sale held", "Other" }).Distinct().ToList();
        StepCombo.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(l => l.Date).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as LienLedgerEntry;
        if (_selected is null) return;
        var idx = _units.FindIndex(u => u.Id == _selected.UnitId);
        if (idx >= 0) UnitPicker.SelectedIndex = idx;
        StepCombo.SelectedItem = _selected.StepType;
        DatePicker.Date = _selected.Date;
        MethodCombo.SelectedItem = _selected.Method;
        TrackingBox.Text = _selected.Tracking;
        NoteBox.Text = _selected.Note;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (UnitPicker.SelectedIndex < 0 || UnitPicker.SelectedIndex >= _units.Count) return;
        var unit = _units[UnitPicker.SelectedIndex];
        var item = _selected ?? new LienLedgerEntry();
        item.UnitId = unit.Id;
        item.UnitLabel = unit.DisplayLine;
        item.StepType = StepCombo.SelectedItem as string ?? "Other";
        item.Date = DatePicker.Date;
        item.Method = MethodCombo.SelectedItem as string ?? "Other";
        item.Tracking = TrackingBox.Text.Trim();
        item.Note = NoteBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(l => l.Id == _selected.Id);
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
        TrackingBox.Text = string.Empty;
        NoteBox.Text = string.Empty;
        DatePicker.Date = DateTimeOffset.Now;
    }
}
