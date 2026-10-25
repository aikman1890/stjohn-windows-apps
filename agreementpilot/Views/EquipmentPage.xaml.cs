using AgreementPilot.Models;
using AgreementPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AgreementPilot.Views;

public sealed partial class EquipmentPage : Page
{
    private readonly JsonListStore<Equipment> _store = new("equipment.json");
    private readonly JsonListStore<TradeCustomer> _customerStore = new("customers.json");
    private readonly JsonListStore<Visit> _visitStore = new("visits.json");
    private readonly TradeSettingsStore _settingsStore = new();
    private List<Equipment> _items = new();
    private List<TradeCustomer> _customers = new();
    private Equipment? _selected;

    public EquipmentPage()
    {
        this.InitializeComponent();
        InstallPicker.Date = DateTimeOffset.Now.AddYears(-5);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        TypeCombo.ItemsSource = settings.EquipmentTypes;
        if (settings.EquipmentTypes.Count > 0) TypeCombo.SelectedIndex = 0;
        _customers = await _customerStore.LoadAsync();
        CustomerPicker.ItemsSource = _customers.Select(c => c.DisplayLine).ToList();
        if (_customers.Count > 0) CustomerPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(eq => eq.CustomerLabel).ThenBy(eq => eq.Type).ToList();
    }

    private async void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Equipment;
        if (_selected is null) return;
        var idx = _customers.FindIndex(c => c.Id == _selected.CustomerId);
        if (idx >= 0) CustomerPicker.SelectedIndex = idx;
        TypeCombo.SelectedItem = _selected.Type;
        BrandBox.Text = _selected.Brand;
        ModelBox.Text = _selected.Model;
        SerialBox.Text = _selected.Serial;
        InstallCheck.IsChecked = _selected.InstallDate is not null;
        if (_selected.InstallDate is { } inst) InstallPicker.Date = inst;
        LocationBox.Text = _selected.LocationNote;
        NotesBox.Text = _selected.Notes;
        var visits = await _visitStore.LoadAsync();
        HistoryList.ItemsSource = visits.Where(v => v.EquipmentId == _selected.Id)
            .OrderByDescending(v => v.Date).ToList();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerPicker.SelectedIndex < 0 || CustomerPicker.SelectedIndex >= _customers.Count) return;
        var customer = _customers[CustomerPicker.SelectedIndex];
        var item = _selected ?? new Equipment();
        item.CustomerId = customer.Id;
        item.CustomerLabel = customer.DisplayLine;
        item.Type = TypeCombo.SelectedItem as string ?? "Other";
        item.Brand = BrandBox.Text.Trim();
        item.Model = ModelBox.Text.Trim();
        item.Serial = SerialBox.Text.Trim();
        item.InstallDate = InstallCheck.IsChecked == true ? InstallPicker.Date : null;
        item.LocationNote = LocationBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(eq => eq.Id == _selected.Id);
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
        BrandBox.Text = string.Empty;
        ModelBox.Text = string.Empty;
        SerialBox.Text = string.Empty;
        InstallCheck.IsChecked = false;
        LocationBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        HistoryList.ItemsSource = null;
    }
}
