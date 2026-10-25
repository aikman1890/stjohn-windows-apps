using BillPilot.Models;
using BillPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BillPilot.Views;

public sealed partial class BillsPage : Page
{
    private static readonly string[] Statuses =
    {
        "Current",
        "Due soon",
        "Past due",
        "Disputed - awaiting response",
        "Disputed - violation",
        "Paid",
        "Closed",
    };

    private readonly JsonListStore<BillItem> _store = new("bills.json");
    private List<BillItem> _items = new();
    private BillItem? _selected;

    public BillsPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = Statuses;
        StatusCombo.SelectedIndex = 0;
        DueDatePicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(b => b.DueDate).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as BillItem;
        if (_selected is null) return;
        ProviderBox.Text = _selected.Provider;
        AccountBox.Text = _selected.AccountLabel;
        AmountDueBox.Text = _selected.AmountDue;
        DueDatePicker.Date = _selected.DueDate;
        DisputedBox.Text = _selected.AmountDisputed;
        StatusCombo.SelectedItem = _selected.Status;
        CaseRefBox.Text = _selected.CaseRef;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProviderBox.Text)) return;
        var item = _selected ?? new BillItem();
        item.Provider = ProviderBox.Text.Trim();
        item.AccountLabel = AccountBox.Text.Trim();
        item.AmountDue = AmountDueBox.Text.Trim();
        item.DueDate = DueDatePicker.Date;
        item.AmountDisputed = DisputedBox.Text.Trim();
        item.Status = StatusCombo.SelectedItem as string ?? Statuses[0];
        item.CaseRef = CaseRefBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(b => b.Id == item.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(b => b.Id == _selected.Id);
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
        ProviderBox.Text = string.Empty;
        AccountBox.Text = string.Empty;
        AmountDueBox.Text = string.Empty;
        DueDatePicker.Date = DateTimeOffset.Now;
        DisputedBox.Text = string.Empty;
        StatusCombo.SelectedIndex = 0;
        CaseRefBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }
}
