using System.Globalization;
using WellPilot.Models;
using WellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WellPilot.Views;

public sealed partial class DecksPage : Page
{
    private readonly JsonListStore<DeckEntry> _store = new("deck.json");
    private readonly JsonListStore<Well> _wellStore = new("wells.json");
    private readonly JsonListStore<InterestOwner> _ownerStore = new("owners.json");
    private List<DeckEntry> _items = new();
    private List<Well> _wells = new();
    private List<InterestOwner> _owners = new();
    private DeckEntry? _selected;

    public DecksPage()
    {
        this.InitializeComponent();
        EffPicker.Date = DateTimeOffset.Now;
        TypeCombo.SelectedIndex = 2;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _wells = await _wellStore.LoadAsync();
        WellPicker.ItemsSource = _wells.Select(w => w.DisplayLine).ToList();
        _owners = await _ownerStore.LoadAsync();
        OwnerPicker.ItemsSource = _owners.Select(o => o.Name).ToList();
        if (_owners.Count > 0) OwnerPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        if (_wells.Count > 0) WellPicker.SelectedIndex = 0;
        RefreshLists();
    }

    private Well? CurrentWell =>
        WellPicker.SelectedIndex >= 0 && WellPicker.SelectedIndex < _wells.Count
            ? _wells[WellPicker.SelectedIndex] : null;

    private void RefreshLists()
    {
        var well = CurrentWell;
        if (well is null)
        {
            ActiveList.ItemsSource = null;
            HistoryList.ItemsSource = null;
            ValidationText.Text = "Add a well first.";
            return;
        }
        var mine = _items.Where(d => d.WellId == well.Id).ToList();
        var active = mine.Where(d => d.Active).OrderBy(d => d.InterestType).ThenBy(d => d.OwnerLabel).ToList();
        ActiveList.ItemsSource = active;
        HistoryList.ItemsSource = mine.Where(d => !d.Active).OrderByDescending(d => d.EffectiveDate).ToList();

        var total = active.Sum(d => d.DecimalInterest);
        var wi = active.Where(d => d.InterestType == "Working interest").Sum(d => d.DecimalInterest);
        var ri = active.Where(d => d.InterestType == "Royalty").Sum(d => d.DecimalInterest);
        var orri = active.Where(d => d.InterestType == "Overriding royalty").Sum(d => d.DecimalInterest);
        ValidationText.Text =
            $"Deck total: {total:0.00000000} (RI {ri:0.00000000} + ORRI {orri:0.00000000} + WI {wi:0.00000000})" +
            (Math.Abs(total - 1m) < 0.000000005m
                ? " - sums to 1.00000000. Good."
                : $" - WARNING: does not sum to 1.00000000 (off by {total - 1m:0.00000000}). Distributions will only allocate the decked share.");
    }

    private void WellPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshLists();

    private void ActiveList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ActiveList.SelectedItem as DeckEntry;
        if (_selected is null) return;
        var idx = _owners.FindIndex(o => o.Id == _selected.OwnerId);
        if (idx >= 0) OwnerPicker.SelectedIndex = idx;
        TypeCombo.SelectedItem = _selected.InterestType;
        DecimalBox.Text = _selected.DecimalInterest.ToString("0.00000000", CultureInfo.InvariantCulture);
        EffPicker.Date = _selected.EffectiveDate;
        NoteBox.Text = _selected.Note;
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var well = CurrentWell;
        if (well is null || OwnerPicker.SelectedIndex < 0 || OwnerPicker.SelectedIndex >= _owners.Count) return;
        var owner = _owners[OwnerPicker.SelectedIndex];
        if (!decimal.TryParse(DecimalBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec)) return;
        var type = TypeCombo.SelectedItem as string ?? "Working interest";

        // Supersede any active line for the same owner+type on this well: versioning, not editing.
        var prior = _items.Where(d => d.WellId == well.Id && d.OwnerId == owner.Id && d.InterestType == type && d.Active).ToList();
        var version = prior.Count > 0 ? prior.Max(d => d.Version) + 1 : 1;
        foreach (var p in prior) p.Active = false;

        _items.Add(new DeckEntry
        {
            WellId = well.Id,
            OwnerId = owner.Id,
            OwnerLabel = owner.Name,
            InterestType = type,
            DecimalInterest = dec,
            EffectiveDate = EffPicker.Date,
            Version = version,
            Active = true,
            Note = NoteBox.Text.Trim(),
        });
        await _store.SaveAsync(_items);
        RefreshLists();
    }

    private async void SupersedeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.Active = false;
        await _store.SaveAsync(_items);
        _selected = null;
        RefreshLists();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(d => d.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        RefreshLists();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ActiveList.SelectedItem = null;
        DecimalBox.Text = string.Empty;
        NoteBox.Text = string.Empty;
    }
}
