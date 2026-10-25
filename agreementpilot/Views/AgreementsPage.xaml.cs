using System.Globalization;
using AgreementPilot.Models;
using AgreementPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AgreementPilot.Views;

public sealed partial class AgreementsPage : Page
{
    private readonly JsonListStore<Agreement> _store = new("agreements.json");
    private readonly JsonListStore<TradeCustomer> _customerStore = new("customers.json");
    private readonly TradeSettingsStore _settingsStore = new();
    private List<Agreement> _items = new();
    private List<TradeCustomer> _customers = new();
    private Agreement? _selected;

    public AgreementsPage()
    {
        this.InitializeComponent();
        StartPicker.Date = DateTimeOffset.Now;
        RenewalPicker.Date = DateTimeOffset.Now.AddYears(1);
        StatusCombo.SelectedIndex = 0;
        PaymentCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        PlanCombo.ItemsSource = settings.PlanNames;
        _customers = await _customerStore.LoadAsync();
        CustomerPicker.ItemsSource = _customers.Select(c => c.DisplayLine).ToList();
        if (_customers.Count > 0) CustomerPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(a => a.RenewalDate).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Agreement;
        if (_selected is null) return;
        var idx = _customers.FindIndex(c => c.Id == _selected.CustomerId);
        if (idx >= 0) CustomerPicker.SelectedIndex = idx;
        PlanCombo.Text = _selected.PlanName;
        PriceBox.Text = _selected.Price.ToString(CultureInfo.InvariantCulture);
        StartPicker.Date = _selected.StartDate;
        RenewalPicker.Date = _selected.RenewalDate;
        IncludedBox.Text = _selected.VisitsIncluded.ToString();
        DeliveredBox.Text = _selected.VisitsDelivered.ToString();
        CoveredBox.Text = _selected.CoveredNote;
        StatusCombo.SelectedItem = _selected.Status;
        PaymentCombo.SelectedItem = _selected.PaymentStatus;
        DiscountsBox.Text = _selected.DiscountsHonored.ToString(CultureInfo.InvariantCulture);
        NotesBox.Text = _selected.Notes;
    }

    private decimal ParseMoney(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerPicker.SelectedIndex < 0 || CustomerPicker.SelectedIndex >= _customers.Count) return;
        var customer = _customers[CustomerPicker.SelectedIndex];
        var item = _selected ?? new Agreement();
        item.CustomerId = customer.Id;
        item.CustomerLabel = customer.DisplayLine;
        item.PlanName = PlanCombo.Text.Trim();
        item.Price = ParseMoney(PriceBox.Text);
        item.StartDate = StartPicker.Date ?? DateTimeOffset.Now;
        item.RenewalDate = RenewalPicker.Date ?? DateTimeOffset.Now.AddYears(1);
        item.VisitsIncluded = int.TryParse(IncludedBox.Text, out var inc) ? inc : 0;
        item.VisitsDelivered = int.TryParse(DeliveredBox.Text, out var del) ? del : 0;
        item.CoveredNote = CoveredBox.Text.Trim();
        item.Status = StatusCombo.SelectedItem as string ?? "Active";
        item.PaymentStatus = PaymentCombo.SelectedItem as string ?? "Paid";
        item.DiscountsHonored = ParseMoney(DiscountsBox.Text);
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(a => a.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
    }

    private async void VisitButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.VisitsDelivered++;
        DeliveredBox.Text = _selected.VisitsDelivered.ToString();
        await _store.SaveAsync(_items);
        RefreshList();
    }

    private async void RenewButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.Status = "Renewed";
        _selected.StartDate = _selected.RenewalDate;
        _selected.RenewalDate = _selected.RenewalDate.AddYears(1);
        _selected.VisitsDelivered = 0;
        await _store.SaveAsync(_items);
        ItemsList_SelectionChanged(this, null!);
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
        PlanCombo.Text = string.Empty;
        PriceBox.Text = string.Empty;
        IncludedBox.Text = string.Empty;
        DeliveredBox.Text = string.Empty;
        CoveredBox.Text = string.Empty;
        DiscountsBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }
}
