using System.Globalization;
using EntityPilot.Models;
using EntityPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EntityPilot.Views;

public sealed partial class OwnershipPage : Page
{
    private readonly JsonListStore<OwnershipStake> _stakeStore = new("ownership.json");
    private readonly JsonListStore<OfficerEntry> _officerStore = new("officers.json");
    private readonly JsonListStore<EntityRecord> _entityStore = new("entities.json");
    private List<OwnershipStake> _stakes = new();
    private List<EntityRecord> _entities = new();
    private OwnershipStake? _stake;

    public OwnershipPage()
    {
        this.InitializeComponent();
        EntityBox.SelectionChanged += async (_, _) => await RefreshAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _entities = (await _entityStore.LoadAsync()).OrderBy(x => x.LegalName).ToList();
        EntityBox.ItemsSource = _entities.Select(x => x.LegalName).ToList();
        _stakes = await _stakeStore.LoadAsync();
        if (_entities.Count > 0) EntityBox.SelectedIndex = 0;
    }

    private EntityRecord? Current =>
        EntityBox.SelectedIndex >= 0 && EntityBox.SelectedIndex < _entities.Count
            ? _entities[EntityBox.SelectedIndex] : null;

    private async Task RefreshAsync()
    {
        if (Current is null) { StakeList.ItemsSource = null; OfficerList.ItemsSource = null; SumText.Text = string.Empty; return; }
        var mine = _stakes.Where(s => s.EntityId == Current.Id)
            .OrderByDescending(s => s.IsActive).ThenBy(s => s.OwnerName).ToList();
        StakeList.ItemsSource = mine;
        var sums = mine.Where(s => s.IsActive).GroupBy(s => s.Class)
            .Select(g => $"{g.Key}: {g.Sum(s => s.Percent):0.00}%{(Math.Abs(g.Sum(s => s.Percent) - 100m) > 0.005m ? "  <-- DOES NOT SUM TO 100%" : "  (sums to 100%)")}");
        SumText.Text = string.Join("   ", sums);
        var officers = await _officerStore.LoadAsync();
        OfficerList.ItemsSource = officers.Where(o => o.EntityId == Current.Id)
            .OrderBy(o => o.Name).Select(o => o.DisplayLine).ToList();
    }

    private void StakeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _stake = StakeList.SelectedItem as OwnershipStake;
        if (_stake is null) return;
        OwnerBox.Text = _stake.OwnerName;
        ClassBox.Text = _stake.Class;
        PercentBox.Text = _stake.Percent.ToString(CultureInfo.InvariantCulture);
        ContribBox.Text = _stake.Contribution.ToString(CultureInfo.InvariantCulture);
        EffPicker.Date = _stake.EffectiveDate;
        StakeNoteBox.Text = _stake.Note;
    }

    private async void SaveStakeButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is null) { ErrorText.Text = "Pick an entity first."; return; }
        if (string.IsNullOrWhiteSpace(OwnerBox.Text)) { ErrorText.Text = "An owner name is required."; return; }
        var s = _stake ?? new OwnershipStake();
        s.EntityId = Current.Id; s.EntityLabel = Current.LegalName;
        s.OwnerName = OwnerBox.Text.Trim();
        s.Class = string.IsNullOrWhiteSpace(ClassBox.Text) ? "Membership" : ClassBox.Text.Trim();
        if (decimal.TryParse(PercentBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var pct)) s.Percent = pct;
        if (decimal.TryParse(ContribBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var con)) s.Contribution = con;
        s.EffectiveDate = EffPicker.Date ?? s.EffectiveDate;
        s.Note = StakeNoteBox.Text.Trim();
        if (_stake is null) _stakes.Add(s);
        await _stakeStore.SaveAsync(_stakes);
        await RefreshAsync();
    }

    private async void SupersedeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_stake is null || !_stake.IsActive) return;
        _stake.SupersededDate = DateTimeOffset.Now;
        await _stakeStore.SaveAsync(_stakes);
        _stake = null;
        await RefreshAsync();
    }

    private void NewStakeButton_Click(object sender, RoutedEventArgs e)
    {
        _stake = null;
        StakeList.SelectedItem = null;
        OwnerBox.Text = PercentBox.Text = ContribBox.Text = StakeNoteBox.Text = string.Empty;
        ClassBox.Text = "Membership";
        EffPicker.Date = DateTimeOffset.Now;
    }

    private async void OfficerButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is null || string.IsNullOrWhiteSpace(OfficerNameBox.Text))
        { ErrorText.Text = "Pick an entity and name the officer."; return; }
        var officers = await _officerStore.LoadAsync();
        officers.Add(new OfficerEntry
        {
            EntityId = Current.Id, EntityLabel = Current.LegalName,
            Name = OfficerNameBox.Text.Trim(), Role = OfficerRoleBox.Text.Trim(),
            AppointedDate = DateTimeOffset.Now,
        });
        await _officerStore.SaveAsync(officers);
        OfficerNameBox.Text = OfficerRoleBox.Text = string.Empty;
        await RefreshAsync();
    }
}
