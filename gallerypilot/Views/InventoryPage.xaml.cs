using System.Globalization;
using GalleryPilot.Models;
using GalleryPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GalleryPilot.Views;

public sealed partial class InventoryPage : Page
{
    private readonly JsonListStore<Artwork> _store = new("artworks.json");
    private readonly JsonListStore<RecordEntry> _recordStore = new("records.json");
    private readonly JsonListStore<Consignment> _consignmentStore = new("consignments.json");
    private List<Artwork> _works = new();
    private List<Consignment> _consignments = new();
    private Artwork? _selected;

    public InventoryPage()
    {
        this.InitializeComponent();
        LocationBox.ItemsSource = new[] { "Gallery", "Storage", "On loan", "With client" };
        StatusBox.ItemsSource = new[] { "Available", "Reserved", "Sold", "Returned" };
        KindBox.ItemsSource = new[] { "Provenance", "Exhibition", "Publication", "Condition" };
        MomentBox.ItemsSource = new[] { "", "Intake", "Before loan", "At return", "General" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _consignments = (await _consignmentStore.LoadAsync()).OrderBy(c => c.ConsignorName).ToList();
        ConsignmentBox.ItemsSource = new[] { "(gallery-owned / none)" }.Concat(_consignments.Select(c => c.ConsignorName)).ToList();
        _works = (await _store.LoadAsync()).OrderBy(w => w.Artist).ThenBy(w => w.Title).ToList();
        WorkList.ItemsSource = _works;
        if (_works.Count > 0) WorkList.SelectedIndex = 0;
    }

    private async void WorkList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = WorkList.SelectedItem as Artwork;
        if (_selected is null) return;
        ArtistBox.Text = _selected.Artist;
        TitleBox.Text = _selected.Title;
        YearBox.Text = _selected.Year;
        MediumBox.Text = _selected.Medium;
        DimsBox.Text = _selected.Dimensions;
        EditionBox.Text = _selected.Edition;
        LocationBox.SelectedItem = _selected.Location;
        StatusBox.SelectedItem = _selected.Status;
        PriceBox.Text = _selected.AskingPrice.ToString(CultureInfo.InvariantCulture);
        CurrencyBox.Text = _selected.Currency;
        var con = _consignments.FirstOrDefault(c => c.Id == _selected.ConsignmentId);
        ConsignmentBox.SelectedItem = con?.ConsignorName ?? "(gallery-owned / none)";
        ImageBox.Text = _selected.ImagePath;
        NotesBox.Text = _selected.Notes;
        ErrorText.Text = string.Empty;
        var records = await _recordStore.LoadAsync();
        RecordList.ItemsSource = records.Where(r => r.ArtworkId == _selected.Id)
            .OrderBy(r => r.Date).ToList();
    }

    private void ReadForm(Artwork a)
    {
        a.Artist = ArtistBox.Text.Trim();
        a.Title = TitleBox.Text.Trim();
        a.Year = YearBox.Text.Trim();
        a.Medium = MediumBox.Text.Trim();
        a.Dimensions = DimsBox.Text.Trim();
        a.Edition = EditionBox.Text.Trim();
        a.Location = LocationBox.SelectedItem as string ?? a.Location;
        a.Status = StatusBox.SelectedItem as string ?? a.Status;
        if (decimal.TryParse(PriceBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) a.AskingPrice = p;
        a.Currency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        var ci = ConsignmentBox.SelectedIndex;
        a.ConsignmentId = ci > 0 && ci - 1 < _consignments.Count ? _consignments[ci - 1].Id : string.Empty;
        a.ImagePath = ImageBox.Text.Trim();
        a.Notes = NotesBox.Text.Trim();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) { ErrorText.Text = "A title is required."; return; }
        var a = _selected ?? new Artwork();
        ReadForm(a);
        if (_selected is null) _works.Add(a);
        await _store.SaveAsync(_works);
        _works = _works.OrderBy(w => w.Artist).ThenBy(w => w.Title).ToList();
        WorkList.ItemsSource = _works;
        WorkList.SelectedItem = _works.FirstOrDefault(w => w.Id == a.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _works.Remove(_selected);
        await _store.SaveAsync(_works);
        _selected = null;
        WorkList.ItemsSource = _works;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        WorkList.SelectedItem = null;
        ArtistBox.Text = TitleBox.Text = YearBox.Text = MediumBox.Text = DimsBox.Text =
            EditionBox.Text = PriceBox.Text = ImageBox.Text = NotesBox.Text = string.Empty;
        CurrencyBox.Text = "USD";
        LocationBox.SelectedItem = "Gallery";
        StatusBox.SelectedItem = "Available";
        RecordList.ItemsSource = null;
    }

    private async void AddEntryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the work first."; return; }
        if (string.IsNullOrWhiteSpace(DetailBox.Text)) { ErrorText.Text = "Write the entry."; return; }
        var records = await _recordStore.LoadAsync();
        records.Add(new RecordEntry
        {
            ArtworkId = _selected.Id,
            ArtworkLabel = $"{_selected.Artist} - {_selected.Title}",
            Kind = KindBox.SelectedItem as string ?? "Provenance",
            Moment = MomentBox.SelectedItem as string ?? string.Empty,
            Detail = DetailBox.Text.Trim(), PhotoPath = PhotoBox.Text.Trim(),
            Date = DateTimeOffset.Now,
        });
        await _recordStore.SaveAsync(records);
        DetailBox.Text = PhotoBox.Text = string.Empty;
        WorkList_SelectionChanged(this, null!);
    }
}
