using LienPilot.Models;
using LienPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LienPilot.Views;

public sealed partial class ClockPage : Page
{
    private readonly JsonListStore<StorageUnit> _unitStore = new("units.json");
    private readonly JsonListStore<StateProfile> _profileStore = new("profiles.json");
    private readonly JsonListStore<LienLedgerEntry> _ledgerStore = new("ledger.json");
    private readonly FacilitySettingsStore _settingsStore = new();
    private List<StorageUnit> _units = new();
    private StateProfile? _profile;
    private List<LienLedgerEntry> _ledger = new();

    public ClockPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var profiles = await _profileStore.LoadAsync();
        _profile = profiles.FirstOrDefault(p => p.Id == settings.ActiveProfileId) ?? profiles.FirstOrDefault();
        _ledger = await _ledgerStore.LoadAsync();
        _units = (await _unitStore.LoadAsync()).Where(u => u.IsDelinquent).OrderBy(u => u.UnitNumber).ToList();
        ProfileText.Text = _profile is null
            ? "No state profile exists yet - create and confirm one in Settings. Without a profile there is no clock."
            : $"Active profile: {_profile.DisplayLine}";
        UnitPicker.ItemsSource = _units.Select(u => u.DisplayLine).ToList();
        if (_units.Count > 0) UnitPicker.SelectedIndex = 0;
        Recompute();
    }

    private void UnitPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => Recompute();

    private void Recompute()
    {
        if (_profile is null || UnitPicker.SelectedIndex < 0 || UnitPicker.SelectedIndex >= _units.Count)
        {
            MilestonesList.ItemsSource = null;
            SaleText.Text = string.Empty;
            BlockText.Text = string.Empty;
            return;
        }
        var unit = _units[UnitPicker.SelectedIndex];
        var tl = LienClock.Compute(unit, _profile, _ledger);
        MilestonesList.ItemsSource = tl.Milestones.Select(m =>
            $"{(m.Done ? "[done]" : "[open]")} {m.Label} - {(m.Date is { } d ? d.ToString("MMM d, yyyy") : "no date")} - {m.Basis}").ToList();
        SaleText.Text = tl.SaleReady
            ? $"SALE READY per your profile and ledger. {tl.SaleBasis}"
            : $"Not sale ready. {tl.SaleBasis}";
        BlockText.Text = tl.BlockReasons.Count > 0
            ? "Why not: " + string.Join(" ", tl.BlockReasons)
            : string.Empty;
    }
}
