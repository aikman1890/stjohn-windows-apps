using System.Globalization;
using ShopPilot.Models;
using ShopPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ShopPilot.Views;

public sealed partial class OutsidePage : Page
{
    private readonly JsonListStore<OutsidePO> _store = new("outsidepos.json");
    private readonly JsonListStore<ShopJob> _jobStore = new("jobs.json");
    private List<OutsidePO> _items = new();
    private List<ShopJob> _jobs = new();
    private OutsidePO? _selected;

    public OutsidePage()
    {
        this.InitializeComponent();
        SentPicker.Date = DateTimeOffset.Now;
        PromisedPicker.Date = DateTimeOffset.Now.AddDays(5);
        ReturnedPicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _jobs = await _jobStore.LoadAsync();
        JobPicker.ItemsSource = _jobs.Select(j => $"{j.JobNumber} - {j.PartDescription}").ToList();
        if (_jobs.Count > 0) JobPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(p => p.ReturnedDate is not null).ThenBy(p => p.PromisedDate).ToList();
    }

    private void ReturnedCheck_Changed(object sender, RoutedEventArgs e)
    {
        ReturnedPicker.IsEnabled = ReturnedCheck.IsChecked == true;
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as OutsidePO;
        if (_selected is null) return;
        var idx = _jobs.FindIndex(j => j.Id == _selected.JobId);
        if (idx >= 0) JobPicker.SelectedIndex = idx;
        VendorBox.Text = _selected.Vendor;
        ServiceBox.Text = _selected.Service;
        PoBox.Text = _selected.PONumber;
        CostBox.Text = _selected.Cost.ToString(CultureInfo.InvariantCulture);
        SentPicker.Date = _selected.SentDate;
        PromisedPicker.Date = _selected.PromisedDate;
        ReturnedCheck.IsChecked = _selected.ReturnedDate is not null;
        if (_selected.ReturnedDate is { } r) ReturnedPicker.Date = r;
        CertBox.Text = _selected.CertRef;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (JobPicker.SelectedIndex < 0 || JobPicker.SelectedIndex >= _jobs.Count) return;
        var job = _jobs[JobPicker.SelectedIndex];
        var item = _selected ?? new OutsidePO();
        item.JobId = job.Id;
        item.JobLabel = $"{job.JobNumber} - {job.PartDescription}";
        item.Vendor = VendorBox.Text.Trim();
        item.Service = ServiceBox.Text.Trim();
        item.PONumber = PoBox.Text.Trim();
        item.Cost = decimal.TryParse(CostBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var c) ? c : 0m;
        item.SentDate = SentPicker.Date ?? DateTimeOffset.Now;
        item.PromisedDate = PromisedPicker.Date ?? DateTimeOffset.Now;
        item.ReturnedDate = ReturnedCheck.IsChecked == true ? ReturnedPicker.Date : null;
        item.CertRef = CertBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(p => p.Id == _selected.Id);
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
        VendorBox.Text = string.Empty;
        ServiceBox.Text = string.Empty;
        PoBox.Text = string.Empty;
        CostBox.Text = string.Empty;
        ReturnedCheck.IsChecked = false;
        CertBox.Text = string.Empty;
    }
}
