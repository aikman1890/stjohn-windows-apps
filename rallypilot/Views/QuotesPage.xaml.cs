using System.Globalization;
using RallyPilot.Models;
using RallyPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RallyPilot.Views;

public sealed partial class QuotesPage : Page
{
    private readonly JsonListStore<Quote> _store = new("quotes.json");
    private readonly JsonListStore<QuoteLine> _lineStore = new("quotelines.json");
    private readonly JsonListStore<EventPackage> _pkgStore = new("packages.json");
    private List<Quote> _quotes = new();
    private List<EventPackage> _packages = new();
    private Quote? _selected;

    public QuotesPage()
    {
        this.InitializeComponent();
        StatusBox.ItemsSource = new[] { "Draft", "Sent", "Won", "Lost", "Booked" };
        EventPicker.Date = DateTimeOffset.Now.AddDays(30);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _packages = (await _pkgStore.LoadAsync()).OrderBy(p => p.Name).ToList();
        PackageBox.ItemsSource = _packages.Select(p => p.Name).ToList();
        _quotes = (await _store.LoadAsync()).OrderByDescending(q => q.EventDate).ToList();
        QuoteList.ItemsSource = _quotes;
        if (_quotes.Count > 0) QuoteList.SelectedIndex = 0;
    }

    private async void QuoteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = QuoteList.SelectedItem as Quote;
        if (_selected is null) { LineList.ItemsSource = null; TotalText.Text = string.Empty; return; }
        CompanyBox.Text = _selected.CompanyLabel;
        OrganizerBox.Text = _selected.OrganizerLabel;
        PackageBox.SelectedItem = _selected.PackageLabel;
        HeadsBox.Text = _selected.Heads.ToString(CultureInfo.InvariantCulture);
        EventPicker.Date = _selected.EventDate;
        TermsBox.Text = _selected.DepositTerms;
        StatusBox.SelectedItem = _selected.Status;
        await RefreshLinesAsync();
    }

    private async Task RefreshLinesAsync()
    {
        if (_selected is null) return;
        var lines = await _lineStore.LoadAsync();
        var mine = lines.Where(l => l.QuoteId == _selected.Id).ToList();
        LineList.ItemsSource = mine;
        TotalText.Text = $"Quote total: {mine.Sum(l => l.LineTotal):0.00} {_selected.Currency} for {_selected.Heads} heads.";
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CompanyBox.Text)) { ErrorText.Text = "Name the company."; return; }
        var q = _selected ?? new Quote();
        q.CompanyLabel = CompanyBox.Text.Trim();
        q.OrganizerLabel = OrganizerBox.Text.Trim();
        q.PackageLabel = PackageBox.SelectedItem as string ?? q.PackageLabel;
        if (int.TryParse(HeadsBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var heads)) q.Heads = heads;
        q.EventDate = EventPicker.Date ?? q.EventDate;
        q.DepositTerms = TermsBox.Text.Trim();
        var newStatus = StatusBox.SelectedItem as string ?? q.Status;
        if (newStatus == "Sent" && q.Status != "Sent") q.SentDate = DateTimeOffset.Now;
        q.Status = newStatus;
        if (_selected is null) _quotes.Add(q);
        await _store.SaveAsync(_quotes);
        QuoteList.ItemsSource = _quotes;
        QuoteList.SelectedItem = _quotes.FirstOrDefault(x => x.Id == q.Id);
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        QuoteList.SelectedItem = null;
        CompanyBox.Text = OrganizerBox.Text = TermsBox.Text = string.Empty;
        HeadsBox.Text = "25";
        EventPicker.Date = DateTimeOffset.Now.AddDays(30);
        StatusBox.SelectedItem = "Draft";
        LineList.ItemsSource = null;
        TotalText.Text = string.Empty;
    }

    private async void LineButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Save/select the quote first."; return; }
        if (string.IsNullOrWhiteSpace(LineLabelBox.Text)) { ErrorText.Text = "Label the line."; return; }
        var lines = await _lineStore.LoadAsync();
        lines.Add(new QuoteLine
        {
            QuoteId = _selected.Id, Label = LineLabelBox.Text.Trim(),
            Qty = decimal.TryParse(LineQtyBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var qty) ? qty : 1m,
            UnitPrice = decimal.TryParse(LinePriceBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ? price : 0m,
        });
        await _lineStore.SaveAsync(lines);
        LineLabelBox.Text = LineQtyBox.Text = LinePriceBox.Text = string.Empty;
        await RefreshLinesAsync();
    }

    private async void WinButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the quote."; return; }
        var lines = await _lineStore.LoadAsync();
        var total = lines.Where(l => l.QuoteId == _selected.Id).Sum(l => l.LineTotal);
        _selected.Status = "Booked";
        await _store.SaveAsync(_quotes);
        var events = await new JsonListStore<RallyEvent>("events.json").LoadAsync();
        if (!events.Any(ev => ev.QuoteId == _selected.Id))
        {
            events.Add(new RallyEvent
            {
                QuoteId = _selected.Id, CompanyLabel = _selected.CompanyLabel,
                OrganizerLabel = _selected.OrganizerLabel, QuoteRef = $"Quote: {_selected.PackageLabel} x{_selected.Heads}",
                EventDate = _selected.EventDate, ExpectedHeads = _selected.Heads, ConfirmedHeads = 0,
                QuotedTotal = total, DepositDue = Math.Round(total / 2m, 2), Currency = _selected.Currency,
                Status = "Planning", Notes = $"Deposit terms: {_selected.DepositTerms}",
            });
            await new JsonListStore<RallyEvent>("events.json").SaveAsync(events);
        }
        QuoteList.ItemsSource = null;
        QuoteList.ItemsSource = _quotes;
        TotalText.Text = $"Event file created for {_selected.CompanyLabel} - quoted total {total:0.00}, deposit due {Math.Round(total / 2m, 2):0.00} (adjust in the event file).";
    }
}
