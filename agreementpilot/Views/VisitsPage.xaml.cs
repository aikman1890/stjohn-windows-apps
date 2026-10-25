using System.Globalization;
using AgreementPilot.Models;
using AgreementPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AgreementPilot.Views;

public sealed partial class VisitsPage : Page
{
    private readonly JsonListStore<Visit> _store = new("visits.json");
    private readonly JsonListStore<Equipment> _equipmentStore = new("equipment.json");
    private List<Visit> _items = new();
    private List<Equipment> _equipment = new();
    private Visit? _selected;

    public VisitsPage()
    {
        this.InitializeComponent();
        DatePickerCtl.Date = DateTimeOffset.Now;
        ResponseCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _equipment = await _equipmentStore.LoadAsync();
        EquipmentPicker.ItemsSource = _equipment.Select(eq => eq.FullLabel).ToList();
        if (_equipment.Count > 0) EquipmentPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(v => v.Date).ToList();
        DeclinedList.ItemsSource = _items
            .Where(v => v.RecResponse == "Declined")
            .OrderBy(v => v.FollowUpDone).ThenBy(v => v.Date)
            .Select(v => $"{(v.FollowUpDone ? "[followed up] " : "[OPEN] ")}{v.CustomerLabel} - {v.Recommendation} ({v.RecAmount:C}) - {v.Date:MMM d, yyyy}")
            .ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Visit;
        if (_selected is null) return;
        var idx = _equipment.FindIndex(eq => eq.Id == _selected.EquipmentId);
        if (idx >= 0) EquipmentPicker.SelectedIndex = idx;
        DatePickerCtl.Date = _selected.Date;
        WorkBox.Text = _selected.WorkPerformed;
        ReadingsBox.Text = _selected.Readings;
        RecBox.Text = _selected.Recommendation;
        ResponseCombo.SelectedItem = _selected.RecResponse;
        RecAmountBox.Text = _selected.RecAmount.ToString(CultureInfo.InvariantCulture);
        FollowCheck.IsChecked = _selected.FollowUpDone;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (EquipmentPicker.SelectedIndex < 0 || EquipmentPicker.SelectedIndex >= _equipment.Count) return;
        var unit = _equipment[EquipmentPicker.SelectedIndex];
        var item = _selected ?? new Visit();
        item.CustomerId = unit.CustomerId;
        item.CustomerLabel = unit.CustomerLabel;
        item.EquipmentId = unit.Id;
        item.EquipmentLabel = unit.FullLabel;
        item.Date = DatePickerCtl.Date ?? DateTimeOffset.Now;
        item.WorkPerformed = WorkBox.Text.Trim();
        item.Readings = ReadingsBox.Text.Trim();
        item.Recommendation = RecBox.Text.Trim();
        item.RecResponse = ResponseCombo.SelectedItem as string ?? "None";
        item.RecAmount = decimal.TryParse(RecAmountBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) ? amt : 0m;
        item.FollowUpDone = FollowCheck.IsChecked == true;
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(v => v.Id == _selected.Id);
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
        WorkBox.Text = string.Empty;
        ReadingsBox.Text = string.Empty;
        RecBox.Text = string.Empty;
        RecAmountBox.Text = string.Empty;
        FollowCheck.IsChecked = false;
    }
}
