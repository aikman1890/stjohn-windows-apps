using System.Globalization;
using ShopPilot.Models;
using ShopPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ShopPilot.Views;

public sealed partial class QuotesPage : Page
{
    private readonly JsonListStore<ShopQuote> _store = new("quotes.json");
    private readonly JsonListStore<ShopCustomer> _customerStore = new("customers.json");
    private readonly JsonListStore<ShopJob> _jobStore = new("jobs.json");
    private List<ShopQuote> _items = new();
    private List<ShopCustomer> _customers = new();
    private ShopQuote? _selected;

    public QuotesPage()
    {
        this.InitializeComponent();
        DatePickerCtl.Date = DateTimeOffset.Now;
        StatusCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _customers = await _customerStore.LoadAsync();
        CustomerPicker.ItemsSource = _customers.Select(c => c.Name).ToList();
        if (_customers.Count > 0) CustomerPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(q => q.Date).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as ShopQuote;
        if (_selected is null) return;
        var idx = _customers.FindIndex(c => c.Id == _selected.CustomerId);
        if (idx >= 0) CustomerPicker.SelectedIndex = idx;
        PartBox.Text = _selected.PartDescription;
        PartNoBox.Text = _selected.PartNumber;
        QtyBox.Text = _selected.Quantity.ToString();
        MaterialBox.Text = _selected.Material;
        BreaksBox.Text = _selected.QtyBreaks;
        PriceBox.Text = _selected.QuotedPrice.ToString(CultureInfo.InvariantCulture);
        LeadBox.Text = _selected.LeadTimeDays.ToString();
        EstMatBox.Text = _selected.EstMaterialCost.ToString(CultureInfo.InvariantCulture);
        EstHoursBox.Text = _selected.EstLaborHours.ToString(CultureInfo.InvariantCulture);
        EstOutBox.Text = _selected.EstOutsideCost.ToString(CultureInfo.InvariantCulture);
        StatusCombo.SelectedItem = _selected.Status;
        LossBox.Text = _selected.LossReason;
        DatePickerCtl.Date = _selected.Date;
    }

    private decimal ParseMoney(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerPicker.SelectedIndex < 0 || CustomerPicker.SelectedIndex >= _customers.Count)
        {
            StatusText.Text = "Add a customer first (Customers & History page).";
            return;
        }
        var customer = _customers[CustomerPicker.SelectedIndex];
        var item = _selected ?? new ShopQuote
        {
            Number = "Q-" + (1000 + _items.Count + 1).ToString(),
        };
        item.CustomerId = customer.Id;
        item.CustomerLabel = customer.Name;
        item.PartDescription = PartBox.Text.Trim();
        item.PartNumber = PartNoBox.Text.Trim();
        item.Quantity = int.TryParse(QtyBox.Text, out var q) ? q : 1;
        item.Material = MaterialBox.Text.Trim();
        item.QtyBreaks = BreaksBox.Text.Trim();
        item.QuotedPrice = ParseMoney(PriceBox.Text);
        item.LeadTimeDays = int.TryParse(LeadBox.Text, out var l) ? l : 0;
        item.EstMaterialCost = ParseMoney(EstMatBox.Text);
        item.EstLaborHours = ParseMoney(EstHoursBox.Text);
        item.EstOutsideCost = ParseMoney(EstOutBox.Text);
        item.Status = StatusCombo.SelectedItem as string ?? "Open";
        item.LossReason = LossBox.Text.Trim();
        item.Date = DatePickerCtl.Date;
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        StatusText.Text = $"Saved {item.Number}.";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(q => q.Id == _selected.Id);
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
        PartBox.Text = string.Empty;
        PartNoBox.Text = string.Empty;
        QtyBox.Text = string.Empty;
        MaterialBox.Text = string.Empty;
        BreaksBox.Text = string.Empty;
        PriceBox.Text = string.Empty;
        LeadBox.Text = string.Empty;
        EstMatBox.Text = string.Empty;
        EstHoursBox.Text = string.Empty;
        EstOutBox.Text = string.Empty;
        LossBox.Text = string.Empty;
        StatusText.Text = string.Empty;
    }

    private async void JobButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { StatusText.Text = "Select a quote first."; return; }
        var jobs = await _jobStore.LoadAsync();
        var job = new ShopJob
        {
            JobNumber = "J-" + (2000 + jobs.Count + 1).ToString(),
            QuoteId = _selected.Id,
            CustomerId = _selected.CustomerId,
            CustomerLabel = _selected.CustomerLabel,
            PartDescription = _selected.PartDescription,
            PartNumber = _selected.PartNumber,
            Material = _selected.Material,
            MaterialCost = _selected.EstMaterialCost,
            Quantity = _selected.Quantity,
            QuotedPrice = _selected.QuotedPrice,
            DueDate = DateTimeOffset.Now.AddDays(Math.Max(1, _selected.LeadTimeDays)),
            Status = "Open",
        };
        jobs.Add(job);
        await _jobStore.SaveAsync(jobs);
        _selected.Status = "Won";
        await _store.SaveAsync(_items);
        RefreshList();
        StatusText.Text = $"Job {job.JobNumber} created from {_selected.Number} - add its operations on the Jobs page.";
    }
}
