using CharterPilot.Models;
using CharterPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CharterPilot.Views;

public sealed partial class ClientsPage : Page
{
    private readonly JsonListStore<BrokerClient> _store = new("clients.json");
    private List<BrokerClient> _clients = new();
    private BrokerClient? _selected;

    public ClientsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _clients = (await _store.LoadAsync()).OrderBy(c => c.Name).ToList();
        ClientList.ItemsSource = _clients.Select(c => c.Name).ToList();
        if (_clients.Count > 0) ClientList.SelectedIndex = 0;
    }

    private async void ClientList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var idx = ClientList.SelectedIndex;
        _selected = idx >= 0 && idx < _clients.Count ? _clients[idx] : null;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        CompanyBox.Text = _selected.Company;
        EmailBox.Text = _selected.Email;
        PhoneBox.Text = _selected.Phone;
        BookerBox.Text = _selected.BookerName;
        BookerContactBox.Text = _selected.BookerContact;
        PrefsBox.Text = _selected.Preferences;
        NotesBox.Text = _selected.Notes;

        var quotes = await new JsonListStore<Quote>("quotes.json").LoadAsync();
        var mine = quotes.Where(q => q.ClientId == _selected.Id).ToList();
        var trips = await new JsonListStore<TripFile>("trips.json").LoadAsync();
        var myTrips = trips.Where(t => t.ClientLabel == _selected.Name).ToList();
        HistoryText.Text = $"Quotes sent: {mine.Count} - booked: {mine.Count(q => q.Status == "Booked")} - lost: {mine.Count(q => q.Status is "Lost" or "Expired")}.\n" +
            $"Lifetime margin kept: {string.Join(", ", myTrips.GroupBy(t => t.Currency).Select(g => $"{g.Sum(t => t.MarginKept):0.00} {g.Key}"))}" +
            (myTrips.Count == 0 ? "none yet" : string.Empty);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A client name is required."; return; }
        var c = _selected ?? new BrokerClient();
        c.Name = NameBox.Text.Trim();
        c.Company = CompanyBox.Text.Trim();
        c.Email = EmailBox.Text.Trim();
        c.Phone = PhoneBox.Text.Trim();
        c.BookerName = BookerBox.Text.Trim();
        c.BookerContact = BookerContactBox.Text.Trim();
        c.Preferences = PrefsBox.Text.Trim();
        c.Notes = NotesBox.Text.Trim();
        if (_selected is null) _clients.Add(c);
        await _store.SaveAsync(_clients);
        _clients = _clients.OrderBy(x => x.Name).ToList();
        ClientList.ItemsSource = _clients.Select(x => x.Name).ToList();
        ClientList.SelectedIndex = _clients.FindIndex(x => x.Id == c.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _clients.Remove(_selected);
        await _store.SaveAsync(_clients);
        _selected = null;
        ClientList.ItemsSource = _clients.Select(x => x.Name).ToList();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ClientList.SelectedItem = null;
        NameBox.Text = CompanyBox.Text = EmailBox.Text = PhoneBox.Text =
            BookerBox.Text = BookerContactBox.Text = PrefsBox.Text = NotesBox.Text = string.Empty;
        HistoryText.Text = string.Empty;
    }
}
