using KennelPilot.Models;
using KennelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace KennelPilot.Views;

public sealed partial class OwnersPage : Page
{
    private readonly JsonListStore<Owner> _store = new("owners.json");
    private List<Owner> _items = new();
    private Owner? _selected;

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

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Owner;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        PhoneBox.Text = _selected.Phone;
        AltPhoneBox.Text = _selected.AltPhone;
        EmailBox.Text = _selected.Email;
        AddressBox.Text = _selected.Address;
        EmergencyBox.Text = _selected.EmergencyContact;
        EmergencyPhoneBox.Text = _selected.EmergencyPhone;
        VetBox.Text = _selected.VetName;
        VetPhoneBox.Text = _selected.VetPhone;
        PayNoteBox.Text = _selected.PaymentNote;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new Owner();
        item.Name = NameBox.Text.Trim();
        item.Phone = PhoneBox.Text.Trim();
        item.AltPhone = AltPhoneBox.Text.Trim();
        item.Email = EmailBox.Text.Trim();
        item.Address = AddressBox.Text.Trim();
        item.EmergencyContact = EmergencyBox.Text.Trim();
        item.EmergencyPhone = EmergencyPhoneBox.Text.Trim();
        item.VetName = VetBox.Text.Trim();
        item.VetPhone = VetPhoneBox.Text.Trim();
        item.PaymentNote = PayNoteBox.Text.Trim();
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
        PhoneBox.Text = string.Empty;
        AltPhoneBox.Text = string.Empty;
        EmailBox.Text = string.Empty;
        AddressBox.Text = string.Empty;
        EmergencyBox.Text = string.Empty;
        EmergencyPhoneBox.Text = string.Empty;
        VetBox.Text = string.Empty;
        VetPhoneBox.Text = string.Empty;
        PayNoteBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }
}
