using DisputePilot.Models;
using DisputePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DisputePilot.Views;

public sealed partial class DisputesPage : Page
{
    private static readonly string[] Statuses =
    {
        "Awaiting validation",
        "Violation - no validation",
        "Partial response",
        "Deleted from reports",
        "Settled",
        "Closed",
    };

    private readonly JsonListStore<DisputeItem> _store = new("disputes.json");
    private List<DisputeItem> _items = new();
    private DisputeItem? _selected;

    public DisputesPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = Statuses;
        StatusCombo.SelectedIndex = 0;
        SentDatePicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(d => d.Deadline).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as DisputeItem;
        if (_selected is null) return;
        CreditorBox.Text = _selected.Creditor;
        AccountBox.Text = _selected.AccountRef;
        AmountBox.Text = _selected.Amount;
        SentDatePicker.Date = _selected.SentDate;
        StatusCombo.SelectedItem = _selected.Status;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CreditorBox.Text)) return;
        var item = _selected ?? new DisputeItem();
        item.Creditor = CreditorBox.Text.Trim();
        item.AccountRef = AccountBox.Text.Trim();
        item.Amount = AmountBox.Text.Trim();
        item.SentDate = SentDatePicker.Date;
        item.Status = StatusCombo.SelectedItem as string ?? Statuses[0];
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(d => d.Id == item.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(d => d.Id == _selected.Id);
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
        CreditorBox.Text = string.Empty;
        AccountBox.Text = string.Empty;
        AmountBox.Text = string.Empty;
        SentDatePicker.Date = DateTimeOffset.Now;
        StatusCombo.SelectedIndex = 0;
        NotesBox.Text = string.Empty;
    }
}
