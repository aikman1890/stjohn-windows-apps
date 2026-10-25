using System.Globalization;
using GalleryPilot.Models;
using GalleryPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GalleryPilot.Views;

public sealed partial class SalesPage : Page
{
    private readonly JsonListStore<Sale> _store = new("sales.json");
    private readonly JsonListStore<Artwork> _workStore = new("artworks.json");
    private readonly JsonListStore<Consignment> _consignmentStore = new("consignments.json");
    private readonly JsonListStore<CollectorContact> _collectorStore = new("collectors.json");
    private List<Sale> _sales = new();
    private List<Artwork> _works = new();
    private List<CollectorContact> _collectors = new();
    private Sale? _selected;

    public SalesPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _works = (await _workStore.LoadAsync()).Where(w => w.Status != "Sold").OrderBy(w => w.Artist).ToList();
        WorkBox.ItemsSource = _works.Select(w => $"{w.Artist} - \"{w.Title}\"").ToList();
        _collectors = (await _collectorStore.LoadAsync()).OrderBy(c => c.Name).ToList();
        CollectorBox.ItemsSource = _collectors.Select(c => c.Name).ToList();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _sales = (await _store.LoadAsync()).OrderByDescending(s => s.Date).ToList();
        SaleList.ItemsSource = _sales;
        var unpaid = _sales.Where(s => !s.IsPaid).ToList();
        OwedText.Text = unpaid.Count == 0
            ? "No artist payables outstanding. Artists are square."
            : "Outstanding: " + string.Join("; ", unpaid.GroupBy(s => s.ArtistLabel)
                .Select(g => $"{g.Key}: {string.Join(", ", g.GroupBy(s => s.Currency).Select(c => $"{c.Sum(s => s.ArtistOwed):0.00} {c.Key}"))}"));
    }

    private void SaleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = SaleList.SelectedItem as Sale;
    }

    private async void RecordButton_Click(object sender, RoutedEventArgs e)
    {
        var wi = WorkBox.SelectedIndex;
        if (wi < 0 || wi >= _works.Count) { ErrorText.Text = "Pick the work that sold."; return; }
        if (!decimal.TryParse(PriceBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
        { ErrorText.Text = "Enter the sale price."; return; }
        var work = _works[wi];
        var settings = await new GallerySettingsStore().LoadAsync();
        var consignments = await _consignmentStore.LoadAsync();
        var con = consignments.FirstOrDefault(c => c.Id == work.ConsignmentId);
        var saleDate = DatePicker.Date ?? DateTimeOffset.Now;
        var sale = new Sale
        {
            ArtworkId = work.Id,
            ArtworkLabel = $"{work.Artist} - \"{work.Title}\"",
            ArtistLabel = con?.ConsignorName ?? work.Artist,
            CollectorLabel = CollectorBox.SelectedItem as string ?? string.Empty,
            Date = saleDate, SalePrice = price,
            Currency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? work.Currency : CurrencyBox.Text.Trim().ToUpperInvariant(),
            SplitPercent = con?.SplitPercent ?? 100m,
            DueDate = saleDate.AddDays(settings.PaymentTermsDays),
            Note = NoteBox.Text.Trim(),
        };
        _sales.Add(sale);
        await _store.SaveAsync(_sales);

        var allWorks = await _workStore.LoadAsync();
        var stored = allWorks.FirstOrDefault(w => w.Id == work.Id);
        if (stored is not null) { stored.Status = "Sold"; await _workStore.SaveAsync(allWorks); }
        _works.Remove(work);
        WorkBox.ItemsSource = _works.Select(w => $"{w.Artist} - \"{w.Title}\"").ToList();
        await ReloadAsync();
        ErrorText.Text = $"Sale recorded. {sale.ArtistLabel} is owed {sale.ArtistOwed:0.00} {sale.Currency} by {sale.DueDate:MMM d, yyyy}.";
    }

    private async void PayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the sale to mark paid."; return; }
        _selected.PaidDate = DateTimeOffset.Now;
        await _store.SaveAsync(_sales);
        await ReloadAsync();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _sales.Remove(_selected);
        await _store.SaveAsync(_sales);
        _selected = null;
        await ReloadAsync();
    }
}
