using System.Globalization;
using CharterPilot.Models;
using CharterPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CharterPilot.Views;

public sealed partial class QuotesPage : Page
{
    private readonly JsonListStore<Quote> _store = new("quotes.json");
    private readonly JsonListStore<BrokerClient> _clientStore = new("clients.json");
    private readonly JsonListStore<Operator> _operatorStore = new("operators.json");
    private List<Quote> _quotes = new();
    private List<BrokerClient> _clients = new();
    private List<Operator> _operators = new();
    private Quote? _selected;

    private static readonly (string Label, decimal Hours)[] Presets =
    {
        ("4 hours", 4m), ("Same day (8 hours)", 8m), ("Next morning (20 hours)", 20m), ("2 days (48 hours)", 48m),
    };

    public QuotesPage()
    {
        this.InitializeComponent();
        StatusBox.ItemsSource = new[] { "Sent", "Follow-up due", "Negotiating", "Booked", "Lost", "Expired" };
        ReasonBox.ItemsSource = new[] { "", "Price", "Timing", "No response", "Aircraft unavailable", "Another broker" };
        PresetBox.ItemsSource = Presets.Select(p => p.Label).ToList();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _clients = (await _clientStore.LoadAsync()).OrderBy(c => c.Name).ToList();
        _operators = (await _operatorStore.LoadAsync()).OrderBy(o => o.Name).ToList();
        ClientBox.ItemsSource = _clients.Select(c => c.Name).ToList();
        OperatorBox.ItemsSource = _operators.Select(o => o.Name).ToList();
        var settings = await new CharterSettingsStore().LoadAsync();
        ClassBox.ItemsSource = settings.AircraftClasses;
        _quotes = (await _store.LoadAsync()).OrderByDescending(q => q.SentAt).ToList();
        QuoteList.ItemsSource = _quotes;
        if (_quotes.Count > 0) QuoteList.SelectedIndex = 0;
    }

    private void QuoteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = QuoteList.SelectedItem as Quote;
        if (_selected is null) return;
        ClientBox.SelectedItem = _selected.ClientLabel;
        DepartPicker.Date = _selected.DepartDate;
        FromBox.Text = _selected.FromText;
        ToBox.Text = _selected.ToText;
        ClassBox.SelectedItem = _selected.AircraftClass;
        OperatorBox.SelectedItem = _selected.OperatorLabel;
        TailBox.Text = _selected.TailLabel;
        CurrencyBox.Text = _selected.Currency;
        CostBox.Text = _selected.Cost.ToString(CultureInfo.InvariantCulture);
        PriceBox.Text = _selected.PriceQuoted.ToString(CultureInfo.InvariantCulture);
        StatusBox.SelectedItem = _selected.Status;
        ReasonBox.SelectedItem = _selected.LostReason;
        NotesBox.Text = _selected.Notes;
        var pi = Array.FindIndex(Presets, p => p.Hours == _selected.FollowUpHours);
        PresetBox.SelectedIndex = pi >= 0 ? pi : 0;
        MarginText.Text = $"Margin (computed): {_selected.Margin:0.00} {_selected.Currency} - follow-up due {_selected.FollowUpDue:MMM d, HH:mm}";
        ErrorText.Text = string.Empty;
    }

    private void ReadForm(Quote q)
    {
        var ci = ClientBox.SelectedIndex;
        if (ci >= 0 && ci < _clients.Count) { q.ClientId = _clients[ci].Id; q.ClientLabel = _clients[ci].Name; }
        var oi = OperatorBox.SelectedIndex;
        if (oi >= 0 && oi < _operators.Count) { q.OperatorId = _operators[oi].Id; q.OperatorLabel = _operators[oi].Name; }
        q.DepartDate = DepartPicker.Date ?? q.DepartDate;
        q.FromText = FromBox.Text.Trim();
        q.ToText = ToBox.Text.Trim();
        q.AircraftClass = ClassBox.SelectedItem as string ?? q.AircraftClass;
        q.TailLabel = TailBox.Text.Trim();
        q.Currency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        if (decimal.TryParse(CostBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var c)) q.Cost = c;
        if (decimal.TryParse(PriceBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) q.PriceQuoted = p;
        q.Status = StatusBox.SelectedItem as string ?? q.Status;
        q.LostReason = ReasonBox.SelectedItem as string ?? string.Empty;
        q.Notes = NotesBox.Text.Trim();
        var pi = PresetBox.SelectedIndex;
        if (pi >= 0 && pi < Presets.Length) q.FollowUpHours = Presets[pi].Hours;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (ClientBox.SelectedIndex < 0) { ErrorText.Text = "Pick the client."; return; }
        var q = _selected ?? new Quote { SentAt = DateTimeOffset.Now, LastTouchedAt = DateTimeOffset.Now };
        ReadForm(q);
        if (_selected is null) _quotes.Add(q);
        await _store.SaveAsync(_quotes);
        _quotes = _quotes.OrderByDescending(x => x.SentAt).ToList();
        QuoteList.ItemsSource = _quotes;
        QuoteList.SelectedItem = _quotes.FirstOrDefault(x => x.Id == q.Id);
    }

    private async void TouchButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.LastTouchedAt = DateTimeOffset.Now;
        if (_selected.Status == "Follow-up due") _selected.Status = "Negotiating";
        await _store.SaveAsync(_quotes);
        QuoteList.ItemsSource = null;
        QuoteList.ItemsSource = _quotes;
        QuoteList.SelectedItem = _selected;
    }

    private async void BookButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the quote to book."; return; }
        _selected.Status = "Booked";
        await _store.SaveAsync(_quotes);
        var trips = await new JsonListStore<TripFile>("trips.json").LoadAsync();
        trips.Add(new TripFile
        {
            QuoteId = _selected.Id, ClientLabel = _selected.ClientLabel,
            RouteLabel = $"{_selected.FromText} -> {_selected.ToText}",
            OperatorLabel = _selected.OperatorLabel, TailLabel = _selected.TailLabel,
            DepartAt = _selected.DepartDate,
            FinalPrice = _selected.PriceQuoted, FinalOperatorCost = _selected.Cost,
            Currency = _selected.Currency,
        });
        await new JsonListStore<TripFile>("trips.json").SaveAsync(trips);
        ErrorText.Text = "Booked - trip file created under Trips with the checklist open.";
        QuoteList.ItemsSource = null;
        QuoteList.ItemsSource = _quotes;
        QuoteList.SelectedItem = _selected;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _quotes.Remove(_selected);
        await _store.SaveAsync(_quotes);
        _selected = null;
        QuoteList.ItemsSource = _quotes;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        QuoteList.SelectedItem = null;
        FromBox.Text = ToBox.Text = TailBox.Text = CostBox.Text = PriceBox.Text = NotesBox.Text = string.Empty;
        CurrencyBox.Text = "USD";
        StatusBox.SelectedItem = "Sent";
        ReasonBox.SelectedItem = "";
        PresetBox.SelectedIndex = 0;
        DepartPicker.Date = DateTimeOffset.Now.AddDays(3);
        MarginText.Text = string.Empty;
    }
}
