using GuardPilot.Models;
using GuardPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GuardPilot.Views;

public sealed partial class GuardsPage : Page
{
    private readonly JsonListStore<Guard> _store = new("guards.json");
    private List<Guard> _items = new();
    private Guard? _selected;

    public GuardsPage()
    {
        this.InitializeComponent();
        StatusCombo.SelectedIndex = 0;
        ExpiryPicker.Date = DateTimeOffset.Now.AddYears(1);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(g => g.Name).ToList();
    }

    private void ExpiryCheck_Changed(object sender, RoutedEventArgs e)
    {
        ExpiryPicker.IsEnabled = ExpiryCheck.IsChecked == true;
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Guard;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        PhoneBox.Text = _selected.Phone;
        EmailBox.Text = _selected.Email;
        LicenseBox.Text = _selected.LicenseNumber;
        ExpiryCheck.IsChecked = _selected.LicenseExpiry is not null;
        if (_selected.LicenseExpiry is { } exp) ExpiryPicker.Date = exp;
        CertsBox.Text = _selected.Certifications;
        StatusCombo.SelectedItem = _selected.Status;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new Guard();
        item.Name = NameBox.Text.Trim();
        item.Phone = PhoneBox.Text.Trim();
        item.Email = EmailBox.Text.Trim();
        item.LicenseNumber = LicenseBox.Text.Trim();
        item.LicenseExpiry = ExpiryCheck.IsChecked == true ? ExpiryPicker.Date : null;
        item.Certifications = CertsBox.Text.Trim();
        item.Status = StatusCombo.SelectedItem as string ?? "Active";
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(g => g.Id == _selected.Id);
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
        LicenseBox.Text = string.Empty;
        ExpiryCheck.IsChecked = false;
        CertsBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }
}
