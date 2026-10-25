using AgreementPilot.Models;
using AgreementPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AgreementPilot.Views;

public sealed partial class CustomersPage : Page
{
    private readonly JsonListStore<TradeCustomer> _store = new("customers.json");
    private readonly JsonListStore<Equipment> _equipmentStore = new("equipment.json");
    private List<TradeCustomer> _items = new();
    private TradeCustomer? _selected;

    public CustomersPage()
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
        ItemsList.ItemsSource = _items.OrderBy(c => c.Name).ToList();
    }

    private async void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as TradeCustomer;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        AddressBox.Text = _selected.Address;
        PhoneBox.Text = _selected.Phone;
        EmailBox.Text = _selected.Email;
        NotesBox.Text = _selected.Notes;
        var equipment = await _equipmentStore.LoadAsync();
        EquipList.ItemsSource = equipment.Where(eq => eq.CustomerId == _selected.Id).ToList();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new TradeCustomer();
        item.Name = NameBox.Text.Trim();
        item.Address = AddressBox.Text.Trim();
        item.Phone = PhoneBox.Text.Trim();
        item.Email = EmailBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(c => c.Id == _selected.Id);
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
        PhoneBox.Text = string.Empty;
        EmailBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        EquipList.ItemsSource = null;
    }
}
