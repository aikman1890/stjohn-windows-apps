using DenialPilot.Models;
using DenialPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DenialPilot.Views;

public sealed partial class CallsPage : Page
{
    private readonly JsonListStore<CallLogEntry> _store = new("calls.json");
    private readonly JsonListStore<DenialCase> _caseStore = new("cases.json");
    private List<CallLogEntry> _items = new();
    private List<DenialCase> _cases = new();
    private CallLogEntry? _selected;

    public CallsPage()
    {
        this.InitializeComponent();
        CallDatePicker.Date = DateTimeOffset.Now;
        CallTimePicker.Time = DateTimeOffset.Now.TimeOfDay;
        FollowUpPicker.Date = DateTimeOffset.Now.AddDays(7);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _cases = await _caseStore.LoadAsync();
        CasePicker.ItemsSource = _cases.Select(c => c.DisplayLine).ToList();
        if (_cases.Count > 0) CasePicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(c => c.CallDate).ToList();
    }

    private DenialCase? PickedCase =>
        CasePicker.SelectedIndex >= 0 && CasePicker.SelectedIndex < _cases.Count
            ? _cases[CasePicker.SelectedIndex]
            : null;

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as CallLogEntry;
        if (_selected is null) return;
        var idx = _cases.FindIndex(c => c.Id == _selected.CaseId);
        if (idx >= 0) CasePicker.SelectedIndex = idx;
        CallDatePicker.Date = _selected.CallDate;
        CallTimePicker.Time = _selected.CallDate.TimeOfDay;
        RepBox.Text = _selected.RepName;
        RefBox.Text = _selected.ReferenceNumber;
        PromisedBox.Text = _selected.Promised;
        FollowUpCheck.IsChecked = _selected.FollowUpDate is not null;
        if (_selected.FollowUpDate is { } f) FollowUpPicker.Date = f;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var picked = PickedCase;
        if (picked is null) return;
        var item = _selected ?? new CallLogEntry();
        item.CaseId = picked.Id;
        item.CaseLabel = picked.DisplayLine;
        item.CallDate = CallDatePicker.Date.Date + CallTimePicker.Time;
        item.RepName = RepBox.Text.Trim();
        item.ReferenceNumber = RefBox.Text.Trim();
        item.Promised = PromisedBox.Text.Trim();
        item.FollowUpDate = FollowUpCheck.IsChecked == true ? FollowUpPicker.Date : null;
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
        CallDatePicker.Date = DateTimeOffset.Now;
        CallTimePicker.Time = DateTimeOffset.Now.TimeOfDay;
        RepBox.Text = string.Empty;
        RefBox.Text = string.Empty;
        PromisedBox.Text = string.Empty;
        FollowUpCheck.IsChecked = false;
        FollowUpPicker.Date = DateTimeOffset.Now.AddDays(7);
        NotesBox.Text = string.Empty;
    }
}
