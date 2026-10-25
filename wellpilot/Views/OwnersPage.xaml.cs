using WellPilot.Models;
using WellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WellPilot.Views;

public sealed partial class OwnersPage : Page
{
    private readonly JsonListStore<InterestOwner> _store = new("owners.json");
    private readonly JsonListStore<StatementLine> _lineStore = new("lines.json");
    private readonly JsonListStore<SuspenseEntry> _suspenseStore = new("suspense.json");
    private readonly JsonListStore<DeckEntry> _deckStore = new("deck.json");
    private List<InterestOwner> _items = new();
    private InterestOwner? _selected;

    public OwnersPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(o => o.Name).ToList();
    }

    private async void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as InterestOwner;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        AddressBox.Text = _selected.Address;
        EmailBox.Text = _selected.Email;
        PhoneBox.Text = _selected.Phone;
        NotesBox.Text = _selected.Notes;

        var lines = (await _lineStore.LoadAsync()).Where(l => l.OwnerId == _selected.Id).ToList();
        var suspense = (await _suspenseStore.LoadAsync()).Where(s => s.OwnerId == _selected.Id).ToList();
        var deck = (await _deckStore.LoadAsync()).Where(d => d.OwnerId == _selected.Id && d.Active).ToList();
        var revenue = lines.Sum(l => l.RevenueCredit);
        var jib = lines.Sum(l => l.JibCharge);
        var held = suspense.Where(s => !s.Released).Sum(s => s.Amount);
        LedgerText.Text =
            $"Active deck lines: {deck.Count}" +
            $"{Environment.NewLine}Revenue distributed (all runs): {revenue:C}" +
            $"{Environment.NewLine}JIB billed (all runs): {jib:C}" +
            $"{Environment.NewLine}Net position: {revenue - jib:C} ({(revenue - jib >= 0 ? "we owe them / paid out" : "they owe the joint account")})" +
            $"{Environment.NewLine}Currently in suspense: {held:C}";
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new InterestOwner();
        item.Name = NameBox.Text.Trim();
        item.Address = AddressBox.Text.Trim();
        item.Email = EmailBox.Text.Trim();
        item.Phone = PhoneBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(o => o.Id == _selected.Id);
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
        NameBox.Text = string.Empty;
        AddressBox.Text = string.Empty;
        EmailBox.Text = string.Empty;
        PhoneBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        LedgerText.Text = string.Empty;
    }
}
