using TaxPilot.Models;
using TaxPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TaxPilot.Views;

public sealed partial class ContactsPage : Page
{
    private readonly JsonListStore<ContactLogEntry> _store = new("contacts.json");
    private readonly JsonListStore<TaxClient> _clientStore = new("clients.json");
    private readonly JsonListStore<Engagement> _engStore = new("engagements.json");
    private readonly TaxSettingsStore _settingsStore = new();
    private List<ContactLogEntry> _items = new();
    private List<TaxClient> _clients = new();
    private ContactLogEntry? _selected;

    public ContactsPage()
    {
        this.InitializeComponent();
        DatePickerCtl.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _clients = await _clientStore.LoadAsync();
        ClientPicker.ItemsSource = _clients.Select(c => c.Name).ToList();
        if (_clients.Count > 0) ClientPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(x => x.Date).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as ContactLogEntry;
        if (_selected is null) return;
        var idx = _clients.FindIndex(c => c.Id == _selected.ClientId);
        if (idx >= 0) ClientPicker.SelectedIndex = idx;
        DatePickerCtl.Date = _selected.Date;
        WhoBox.Text = _selected.Who;
        SummaryBox.Text = _selected.Summary;
        PromisedBox.Text = _selected.Promised;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (ClientPicker.SelectedIndex < 0 || ClientPicker.SelectedIndex >= _clients.Count) return;
        var client = _clients[ClientPicker.SelectedIndex];
        var item = _selected ?? new ContactLogEntry();
        item.ClientId = client.Id;
        item.ClientLabel = client.Name;
        item.Date = DatePickerCtl.Date ?? DateTimeOffset.Now;
        item.Who = WhoBox.Text.Trim();
        item.Summary = SummaryBox.Text.Trim();
        item.Promised = PromisedBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);

        var settings = await _settingsStore.LoadAsync();
        var engagements = await _engStore.LoadAsync();
        var eng = engagements.FirstOrDefault(x => x.ClientId == client.Id && x.TaxYear == settings.CurrentTaxYear);
        if (eng is not null)
        {
            eng.LastContacted = item.Date;
            await _engStore.SaveAsync(engagements);
        }
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(x => x.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
    }

    private void ClearForm()
    {
        WhoBox.Text = string.Empty;
        SummaryBox.Text = string.Empty;
        PromisedBox.Text = string.Empty;
    }
}
