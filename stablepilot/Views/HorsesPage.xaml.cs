using System.Globalization;
using StablePilot.Models;
using StablePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace StablePilot.Views;

public sealed partial class HorsesPage : Page
{
    private readonly JsonListStore<Horse> _store = new("horses.json");
    private readonly JsonListStore<HorseOwner> _ownerStore = new("owners.json");
    private readonly JsonListStore<HorseDoc> _docStore = new("documents.json");
    private List<Horse> _horses = new();
    private List<HorseOwner> _owners = new();
    private Horse? _selected;

    public HorsesPage()
    {
        this.InitializeComponent();
        DocKindBox.ItemsSource = new[] { "Coggins certificate", "Health papers", "Registration", "Insurance", "Other" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _owners = (await _ownerStore.LoadAsync()).OrderBy(o => o.Name).ToList();
        OwnerBox.ItemsSource = _owners.Select(o => o.Name).ToList();
        OwnerList.ItemsSource = _owners.Select(o => $"{o.Name} - {o.Contact}").ToList();
        _horses = (await _store.LoadAsync()).OrderBy(h => h.Name).ToList();
        HorseList.ItemsSource = _horses;
        if (_horses.Count > 0) HorseList.SelectedIndex = 0;
    }

    private async void HorseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = HorseList.SelectedItem as Horse;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        OwnerBox.SelectedItem = _selected.OwnerLabel;
        StallBox.Text = _selected.Stall;
        BreedBox.Text = _selected.BreedAgeNote;
        FeedAmBox.Text = _selected.FeedAm;
        FeedPmBox.Text = _selected.FeedPm;
        SuppBox.Text = _selected.Supplements;
        TurnoutBox.Text = _selected.TurnoutGroup;
        BlanketBox.Text = _selected.BlanketRule;
        RateBox.Text = _selected.BoardRateMonthly.ToString(CultureInfo.InvariantCulture);
        VetBox.Text = _selected.VetLabel;
        FarrierBox.Text = _selected.FarrierLabel;
        NotesBox.Text = _selected.Notes;
        var clocks = await new JsonListStore<CareClock>("clocks.json").LoadAsync();
        var mine = clocks.Where(c => c.HorseId == _selected.Id).OrderBy(c => c.NextDueDate).ToList();
        ClockSummaryText.Text = mine.Count == 0 ? "No care clocks for this horse yet."
            : "Clocks: " + string.Join(" | ", mine.Select(c => $"{c.Kind} {(c.DaysUntil < 0 ? $"OVERDUE {-c.DaysUntil}d" : $"in {c.DaysUntil}d")}"));
        var docs = await _docStore.LoadAsync();
        DocList.ItemsSource = docs.Where(d => d.HorseId == _selected.Id).Select(d => d.DisplayLine).ToList();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A horse name is required."; return; }
        if (OwnerBox.SelectedIndex < 0) { ErrorText.Text = "Pick the owner (add one at left if needed)."; return; }
        var h = _selected ?? new Horse();
        var owner = _owners[OwnerBox.SelectedIndex];
        h.OwnerId = owner.Id; h.OwnerLabel = owner.Name;
        h.Name = NameBox.Text.Trim();
        h.Stall = StallBox.Text.Trim();
        h.BreedAgeNote = BreedBox.Text.Trim();
        h.FeedAm = FeedAmBox.Text.Trim();
        h.FeedPm = FeedPmBox.Text.Trim();
        h.Supplements = SuppBox.Text.Trim();
        h.TurnoutGroup = TurnoutBox.Text.Trim();
        h.BlanketRule = BlanketBox.Text.Trim();
        if (decimal.TryParse(RateBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate)) h.BoardRateMonthly = rate;
        h.VetLabel = VetBox.Text.Trim();
        h.FarrierLabel = FarrierBox.Text.Trim();
        h.Notes = NotesBox.Text.Trim();
        if (_selected is null) { h.ArrivalDate = DateTimeOffset.Now; _horses.Add(h); }
        await _store.SaveAsync(_horses);
        _horses = _horses.OrderBy(x => x.Name).ToList();
        HorseList.ItemsSource = _horses;
        HorseList.SelectedItem = _horses.FirstOrDefault(x => x.Id == h.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _horses.Remove(_selected);
        await _store.SaveAsync(_horses);
        _selected = null;
        HorseList.ItemsSource = _horses;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        HorseList.SelectedItem = null;
        NameBox.Text = StallBox.Text = BreedBox.Text = FeedAmBox.Text = FeedPmBox.Text =
            SuppBox.Text = TurnoutBox.Text = BlanketBox.Text = RateBox.Text = VetBox.Text =
            FarrierBox.Text = NotesBox.Text = string.Empty;
        ClockSummaryText.Text = string.Empty;
        DocList.ItemsSource = null;
    }

    private async void OwnerButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(OwnerNameBox.Text)) { ErrorText.Text = "Name the owner."; return; }
        _owners.Add(new HorseOwner { Name = OwnerNameBox.Text.Trim(), Contact = OwnerContactBox.Text.Trim() });
        await _ownerStore.SaveAsync(_owners);
        _owners = _owners.OrderBy(o => o.Name).ToList();
        OwnerBox.ItemsSource = _owners.Select(o => o.Name).ToList();
        OwnerList.ItemsSource = _owners.Select(o => $"{o.Name} - {o.Contact}").ToList();
        OwnerNameBox.Text = OwnerContactBox.Text = string.Empty;
    }

    private async void DocButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the horse first."; return; }
        var docs = await _docStore.LoadAsync();
        docs.Add(new HorseDoc
        {
            HorseId = _selected.Id, HorseLabel = _selected.Name,
            Kind = DocKindBox.SelectedItem as string ?? "Other",
            Title = DocTitleBox.Text.Trim(), LocalPath = DocPathBox.Text.Trim(),
        });
        await _docStore.SaveAsync(docs);
        DocTitleBox.Text = DocPathBox.Text = string.Empty;
        HorseList_SelectionChanged(this, null!);
    }
}
