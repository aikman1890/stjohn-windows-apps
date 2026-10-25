using WellPilot.Models;
using WellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WellPilot.Views;

public sealed partial class WellsPage : Page
{
    private readonly JsonListStore<Well> _store = new("wells.json");
    private List<Well> _items = new();
    private Well? _selected;

    public WellsPage()
    {
        this.InitializeComponent();
        StatusCombo.SelectedIndex = 0;
        FirstPicker.Date = DateTimeOffset.Now.AddYears(-5);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(w => w.LeaseName).ThenBy(w => w.WellName).ToList();
    }

    private void FirstCheck_Changed(object sender, RoutedEventArgs e)
    {
        FirstPicker.IsEnabled = FirstCheck.IsChecked == true;
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Well;
        if (_selected is null) return;
        LeaseBox.Text = _selected.LeaseName;
        WellBox.Text = _selected.WellName;
        ApiBox.Text = _selected.ApiNumber;
        CountyBox.Text = _selected.County;
        StateBox.Text = _selected.State;
        OperatorBox.Text = _selected.Operator;
        StatusCombo.SelectedItem = _selected.Status;
        FirstCheck.IsChecked = _selected.FirstProductionDate is not null;
        if (_selected.FirstProductionDate is { } f) FirstPicker.Date = f;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new Well();
        item.LeaseName = LeaseBox.Text.Trim();
        item.WellName = WellBox.Text.Trim();
        item.ApiNumber = ApiBox.Text.Trim();
        item.County = CountyBox.Text.Trim();
        item.State = StateBox.Text.Trim();
        item.Operator = OperatorBox.Text.Trim();
        item.Status = StatusCombo.SelectedItem as string ?? "Producing";
        item.FirstProductionDate = FirstCheck.IsChecked == true ? FirstPicker.Date : null;
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(w => w.Id == _selected.Id);
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
        LeaseBox.Text = string.Empty;
        WellBox.Text = string.Empty;
        ApiBox.Text = string.Empty;
        CountyBox.Text = string.Empty;
        OperatorBox.Text = string.Empty;
        FirstCheck.IsChecked = false;
        NotesBox.Text = string.Empty;
    }
}
