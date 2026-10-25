using System.Globalization;
using MovePilot.Models;
using MovePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MovePilot.Views;

public sealed partial class EstimatesPage : Page
{
    private readonly JsonListStore<MoveEstimate> _store = new("estimates.json");
    private readonly JsonListStore<MoveJob> _moveStore = new("moves.json");
    private readonly MoveSettingsStore _settingsStore = new();
    private List<MoveEstimate> _items = new();
    private List<MoveJob> _moves = new();
    private List<string> _terms = new();
    private MoveEstimate? _selected;

    public EstimatesPage()
    {
        this.InitializeComponent();
        TypeCombo.SelectedIndex = 0;
        StatusCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        _terms = settings.TermsPresets;
        TermsPicker.ItemsSource = _terms;
        RateBox.Text = settings.DefaultHourlyRate.ToString(CultureInfo.InvariantCulture);
        CrewBox.Text = settings.DefaultCrewSize.ToString();
        TrucksBox.Text = "1";
        _moves = await _moveStore.LoadAsync();
        MovePicker.ItemsSource = _moves.Select(m => m.Customer).ToList();
        if (_moves.Count > 0) MovePicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(x => x.CreatedDate).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as MoveEstimate;
        if (_selected is null) return;
        var idx = _moves.FindIndex(m => m.Id == _selected.MoveId);
        if (idx >= 0) MovePicker.SelectedIndex = idx;
        TypeCombo.SelectedItem = _selected.Type;
        CrewBox.Text = _selected.CrewSize.ToString();
        TrucksBox.Text = _selected.Trucks.ToString();
        RateBox.Text = _selected.HourlyRate.ToString(CultureInfo.InvariantCulture);
        HoursBox.Text = _selected.EstHours.ToString(CultureInfo.InvariantCulture);
        FlatBox.Text = _selected.FlatAmount.ToString(CultureInfo.InvariantCulture);
        TravelBox.Text = _selected.TravelFee.ToString(CultureInfo.InvariantCulture);
        MaterialsBox.Text = _selected.Materials.ToString(CultureInfo.InvariantCulture);
        OtherBox.Text = _selected.OtherFees.ToString(CultureInfo.InvariantCulture);
        TermsBox.Text = _selected.Terms;
        StatusCombo.SelectedItem = _selected.Status;
        DecidedByBox.Text = _selected.DecidedBy;
        MethodBox.Text = _selected.DecisionMethod;
        AmountText.Text = $"Amount: {_selected.Amount:C}";
    }

    private decimal ParseMoney(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private void CalcButton_Click(object sender, RoutedEventArgs e)
    {
        var probe = new MoveEstimate
        {
            Type = TypeCombo.SelectedItem as string ?? "Hourly",
            HourlyRate = ParseMoney(RateBox.Text), EstHours = ParseMoney(HoursBox.Text),
            FlatAmount = ParseMoney(FlatBox.Text), TravelFee = ParseMoney(TravelBox.Text),
            Materials = ParseMoney(MaterialsBox.Text), OtherFees = ParseMoney(OtherBox.Text),
        };
        AmountText.Text = $"Amount: {probe.Amount:C}";
    }

    private void TermsPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TermsPicker.SelectedIndex >= 0 && TermsPicker.SelectedIndex < _terms.Count)
            TermsBox.Text = _terms[TermsPicker.SelectedIndex];
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (MovePicker.SelectedIndex < 0 || MovePicker.SelectedIndex >= _moves.Count) return;
        var move = _moves[MovePicker.SelectedIndex];
        var version = _items.Where(x => x.MoveId == move.Id).Select(x => x.Version).DefaultIfEmpty(0).Max() + 1;
        var item = new MoveEstimate
        {
            MoveId = move.Id,
            CustomerLabel = move.Customer,
            Version = (_selected is not null && _selected.MoveId == move.Id) ? _selected.Version : version,
            Type = TypeCombo.SelectedItem as string ?? "Hourly",
            CrewSize = int.TryParse(CrewBox.Text, out var c) ? c : 0,
            Trucks = int.TryParse(TrucksBox.Text, out var t) ? t : 0,
            HourlyRate = ParseMoney(RateBox.Text),
            EstHours = ParseMoney(HoursBox.Text),
            FlatAmount = ParseMoney(FlatBox.Text),
            TravelFee = ParseMoney(TravelBox.Text),
            Materials = ParseMoney(MaterialsBox.Text),
            OtherFees = ParseMoney(OtherBox.Text),
            Terms = TermsBox.Text.Trim(),
            Status = StatusCombo.SelectedItem as string ?? "Draft",
            CreatedDate = DateTimeOffset.Now,
        };
        // Saving from a selected estimate edits it in place; use "Save as new version" flow by
        // pressing Clear first for a fresh version. If a selection exists, update it.
        if (_selected is not null)
        {
            item.Id = _selected.Id;
            item.Version = _selected.Version;
            item.DecisionDate = _selected.DecisionDate;
            item.DecisionMethod = _selected.DecisionMethod;
            item.DecidedBy = _selected.DecidedBy;
            item.CreatedDate = _selected.CreatedDate;
            _items[_items.FindIndex(x => x.Id == item.Id)] = item;
        }
        else
        {
            _items.Add(item);
        }
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(x => x.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        RefreshList();
    }

    private async void ApproveButton_Click(object sender, RoutedEventArgs e) => await LogDecisionAsync("Approved");

    private async void DeclineButton_Click(object sender, RoutedEventArgs e) => await LogDecisionAsync("Declined");

    private async Task LogDecisionAsync(string decision)
    {
        if (_selected is null) return;
        _selected.Status = decision;
        _selected.DecisionDate = DateTimeOffset.Now;
        _selected.DecidedBy = DecidedByBox.Text.Trim();
        _selected.DecisionMethod = MethodBox.Text.Trim();
        await _store.SaveAsync(_items);
        if (decision == "Approved")
        {
            var moves = await _moveStore.LoadAsync();
            var move = moves.FirstOrDefault(m => m.Id == _selected.MoveId);
            if (move is not null)
            {
                move.ContractAmount = _selected.Amount;
                if (move.Status is "Lead" or "Surveyed") move.Status = "Booked";
                await _moveStore.SaveAsync(moves);
            }
        }
        RefreshList();
    }
}
