using RemodelPilot.Models;
using RemodelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RemodelPilot.Views;

public sealed partial class JobsPage : Page
{
    private readonly JsonListStore<Job> _store = new("jobs.json");
    private readonly JsonListStore<ChangeOrder> _coStore = new("changeorders.json");
    private readonly JsonListStore<ActualCost> _actualStore = new("actuals.json");
    private readonly JsonListStore<Draw> _drawStore = new("draws.json");
    private List<Job> _items = new();
    private Job? _selected;

    public JobsPage()
    {
        this.InitializeComponent();
        TypeCombo.ItemsSource = new[] { "Fixed price", "Cost plus", "Time & materials" };
        TypeCombo.SelectedIndex = 0;
        StatusCombo.ItemsSource = new[] { "Bid", "Won", "In progress", "Punch list", "Complete", "Warranty", "Closed" };
        StatusCombo.SelectedIndex = 0;
        StartPicker.Date = DateTimeOffset.Now;
        CompletePicker.Date = DateTimeOffset.Now;
        WarrantyPicker.Date = DateTimeOffset.Now.AddYears(1);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(j => j.StartDate).ToList();
    }

    private async void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Job;
        if (_selected is null) return;
        ClientBox.Text = _selected.ClientName;
        AddressBox.Text = _selected.Address;
        PhoneBox.Text = _selected.Phone;
        EmailBox.Text = _selected.Email;
        TypeCombo.SelectedItem = _selected.ContractType;
        StatusCombo.SelectedItem = _selected.Status;
        if (_selected.StartDate is { } sd) StartPicker.Date = sd;
        CompleteCheck.IsChecked = _selected.CompleteDate is not null;
        if (_selected.CompleteDate is { } cd) CompletePicker.Date = cd;
        WarrantyCheck.IsChecked = _selected.WarrantyEnd is not null;
        if (_selected.WarrantyEnd is { } wd) WarrantyPicker.Date = wd;
        NotesBox.Text = _selected.Notes;

        var cos = await _coStore.LoadAsync();
        var actuals = await _actualStore.LoadAsync();
        var draws = await _drawStore.LoadAsync();
        var revenue = JobMath.ContractPrice(_selected, cos);
        var cost = JobMath.ActualTotal(actuals, _selected.Id);
        var drawn = draws.Where(d => d.JobId == _selected.Id && d.PaidDate is not null).Sum(d => d.Amount);
        MoneyText.Text =
            $"Contract (base {_selected.BaseContractPrice:C} + approved COs {cos.Where(c => c.JobId == _selected.Id && c.Status == "Approved").Sum(c => c.Price):C}) = {revenue:C}" +
            $"{Environment.NewLine}Actual costs {cost:C} - margin {revenue - cost:C} - draws paid {drawn:C}";
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new Job();
        item.ClientName = ClientBox.Text.Trim();
        item.Address = AddressBox.Text.Trim();
        item.Phone = PhoneBox.Text.Trim();
        item.Email = EmailBox.Text.Trim();
        item.ContractType = TypeCombo.SelectedItem as string ?? "Fixed price";
        item.Status = StatusCombo.SelectedItem as string ?? "Bid";
        item.StartDate = StartPicker.Date;
        item.CompleteDate = CompleteCheck.IsChecked == true ? CompletePicker.Date : null;
        item.WarrantyEnd = WarrantyCheck.IsChecked == true ? WarrantyPicker.Date : null;
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(j => j.Id == _selected.Id);
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
        ClientBox.Text = string.Empty;
        AddressBox.Text = string.Empty;
        PhoneBox.Text = string.Empty;
        EmailBox.Text = string.Empty;
        TypeCombo.SelectedIndex = 0;
        StatusCombo.SelectedIndex = 0;
        CompleteCheck.IsChecked = false;
        WarrantyCheck.IsChecked = false;
        NotesBox.Text = string.Empty;
        MoneyText.Text = string.Empty;
    }
}
