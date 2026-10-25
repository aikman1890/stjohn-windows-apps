using GalleryPilot.Models;
using GalleryPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GalleryPilot.Views;

public sealed partial class CollectorsPage : Page
{
    private readonly JsonListStore<CollectorContact> _store = new("collectors.json");
    private readonly JsonListStore<OfferEntry> _offerStore = new("offers.json");
    private List<CollectorContact> _collectors = new();
    private CollectorContact? _selected;

    public CollectorsPage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = new[] { "Bought", "Offered", "Viewing" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _collectors = (await _store.LoadAsync()).OrderBy(c => c.Name).ToList();
        CollectorList.ItemsSource = _collectors.Select(c => c.Name).ToList();
        if (_collectors.Count > 0) CollectorList.SelectedIndex = 0;
    }

    private async void CollectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var idx = CollectorList.SelectedIndex;
        _selected = idx >= 0 && idx < _collectors.Count ? _collectors[idx] : null;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        ContactBox.Text = _selected.Contact;
        InterestsBox.Text = _selected.Interests;
        NotesBox.Text = _selected.Notes;
        var offers = await _offerStore.LoadAsync();
        HistoryList.ItemsSource = offers.Where(o => o.CollectorId == _selected.Id)
            .OrderByDescending(o => o.Date).Select(o => o.DisplayLine).ToList();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A name is required."; return; }
        var c = _selected ?? new CollectorContact();
        c.Name = NameBox.Text.Trim();
        c.Contact = ContactBox.Text.Trim();
        c.Interests = InterestsBox.Text.Trim();
        c.Notes = NotesBox.Text.Trim();
        if (_selected is null) _collectors.Add(c);
        await _store.SaveAsync(_collectors);
        _collectors = _collectors.OrderBy(x => x.Name).ToList();
        CollectorList.ItemsSource = _collectors.Select(x => x.Name).ToList();
        CollectorList.SelectedIndex = _collectors.FindIndex(x => x.Id == c.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _collectors.Remove(_selected);
        await _store.SaveAsync(_collectors);
        _selected = null;
        CollectorList.ItemsSource = _collectors.Select(x => x.Name).ToList();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        CollectorList.SelectedItem = null;
        NameBox.Text = ContactBox.Text = InterestsBox.Text = NotesBox.Text = string.Empty;
        HistoryList.ItemsSource = null;
    }

    private async void HistButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the collector first."; return; }
        var offers = await _offerStore.LoadAsync();
        offers.Add(new OfferEntry
        {
            CollectorId = _selected.Id, CollectorLabel = _selected.Name,
            ArtworkLabel = WorkLabelBox.Text.Trim(),
            Kind = KindBox.SelectedItem as string ?? "Viewing",
            Note = HistNoteBox.Text.Trim(), Date = DateTimeOffset.Now,
        });
        await _offerStore.SaveAsync(offers);
        WorkLabelBox.Text = HistNoteBox.Text = string.Empty;
        CollectorList_SelectionChanged(this, null!);
    }
}
