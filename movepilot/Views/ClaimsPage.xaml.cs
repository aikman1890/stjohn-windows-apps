using System.Globalization;
using MovePilot.Models;
using MovePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MovePilot.Views;

public sealed partial class ClaimsPage : Page
{
    private readonly JsonListStore<MoveClaim> _store = new("claims.json");
    private readonly JsonListStore<MoveJob> _moveStore = new("moves.json");
    private readonly MoveSettingsStore _settingsStore = new();
    private List<MoveClaim> _items = new();
    private List<MoveJob> _moves = new();
    private MoveClaim? _selected;

    public ClaimsPage()
    {
        this.InitializeComponent();
        FiledPicker.Date = DateTimeOffset.Now;
        StatusCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        ClockBox.Text = settings.DefaultClaimAckDays.ToString();
        _moves = await _moveStore.LoadAsync();
        MovePicker.ItemsSource = _moves.Select(m => m.Customer).ToList();
        if (_moves.Count > 0) MovePicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(c => c.AckDue).ToList();
        AckText.Text = _selected is null
            ? string.Empty
            : $"Acknowledge by {_selected.AckDue:MMM d, yyyy} ({_selected.DaysToAck} days)" +
              (_selected.AckDate is { } a ? $" - acknowledged {a:MMM d, yyyy}" : string.Empty);
        TrailText.Text = _selected?.ProofLog ?? string.Empty;
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as MoveClaim;
        if (_selected is null) return;
        var idx = _moves.FindIndex(m => m.Id == _selected.MoveId);
        if (idx >= 0) MovePicker.SelectedIndex = idx;
        ItemBox.Text = _selected.Item;
        AmountBox.Text = _selected.AmountClaimed.ToString(CultureInfo.InvariantCulture);
        FiledPicker.Date = _selected.FiledDate;
        ClockBox.Text = _selected.AckClockDays.ToString();
        OfferBox.Text = _selected.OfferAmount.ToString(CultureInfo.InvariantCulture);
        SettledBox.Text = _selected.SettledAmount.ToString(CultureInfo.InvariantCulture);
        StatusCombo.SelectedItem = _selected.Status;
        RefreshList();
    }

    private decimal ParseMoney(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (MovePicker.SelectedIndex < 0 || MovePicker.SelectedIndex >= _moves.Count) return;
        var move = _moves[MovePicker.SelectedIndex];
        var item = _selected ?? new MoveClaim();
        item.MoveId = move.Id;
        item.CustomerLabel = move.Customer;
        item.Item = ItemBox.Text.Trim();
        item.AmountClaimed = ParseMoney(AmountBox.Text);
        item.FiledDate = FiledPicker.Date;
        item.AckClockDays = int.TryParse(ClockBox.Text, out var d) ? d : 30;
        item.OfferAmount = ParseMoney(OfferBox.Text);
        item.SettledAmount = ParseMoney(SettledBox.Text);
        item.Status = StatusCombo.SelectedItem as string ?? "Open";
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
        RefreshList();
    }

    private void ClearForm()
    {
        ItemBox.Text = string.Empty;
        AmountBox.Text = string.Empty;
        OfferBox.Text = string.Empty;
        SettledBox.Text = string.Empty;
        ProofBox.Text = string.Empty;
    }

    private async Task PersistAsync()
    {
        await _store.SaveAsync(_items);
        RefreshList();
    }

    private async void AckButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.AckDate = DateTimeOffset.Now;
        _selected.Status = "Acknowledged";
        await PersistAsync();
    }

    private async void InspectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.InspectDate = DateTimeOffset.Now;
        _selected.Status = "Inspected";
        await PersistAsync();
    }

    private async void OfferButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.OfferDate = DateTimeOffset.Now;
        _selected.OfferAmount = ParseMoney(OfferBox.Text);
        _selected.Status = "Offered";
        await PersistAsync();
    }

    private async void SettleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.SettledDate = DateTimeOffset.Now;
        _selected.SettledAmount = ParseMoney(SettledBox.Text);
        _selected.Status = "Settled";
        await PersistAsync();
    }

    private async void DeclineButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.Status = "Declined";
        await PersistAsync();
    }

    private async void ProofButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null || string.IsNullOrWhiteSpace(ProofBox.Text)) return;
        var line = $"{DateTimeOffset.Now:yyyy-MM-dd} {ProofBox.Text.Trim()}";
        _selected.ProofLog = string.IsNullOrWhiteSpace(_selected.ProofLog)
            ? line
            : _selected.ProofLog + Environment.NewLine + line;
        ProofBox.Text = string.Empty;
        await PersistAsync();
    }
}
