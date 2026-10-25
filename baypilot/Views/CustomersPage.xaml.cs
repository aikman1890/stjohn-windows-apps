using BayPilot.Models;
using BayPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BayPilot.Views;

public sealed partial class CustomersPage : Page
{
    private readonly JsonListStore<Customer> _store = new("customers.json");
    private readonly JsonListStore<Vehicle> _vehicleStore = new("vehicles.json");
    private List<Customer> _items = new();
    private Customer? _selected;

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
        _selected = ItemsList.SelectedItem as Customer;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        PhoneBox.Text = _selected.Phone;
        EmailBox.Text = _selected.Email;
        AddressBox.Text = _selected.Address;
        NotesBox.Text = _selected.Notes;
        var vehicles = (await _vehicleStore.LoadAsync()).Where(v => v.CustomerId == _selected.Id).ToList();
        VehiclesText.Text = vehicles.Count == 0
            ? "None yet - add them on the Vehicles page."
            : string.Join(Environment.NewLine, vehicles.Select(v => $"{v.DisplayLine} - {v.SubLine}"));
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) return;
        var item = _selected ?? new Customer();
        item.Name = NameBox.Text.Trim();
        item.Phone = PhoneBox.Text.Trim();
        item.Email = EmailBox.Text.Trim();
        item.Address = AddressBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(c => c.Id == item.Id);
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
        PhoneBox.Text = string.Empty;
        EmailBox.Text = string.Empty;
        AddressBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        VehiclesText.Text = string.Empty;
    }
}
