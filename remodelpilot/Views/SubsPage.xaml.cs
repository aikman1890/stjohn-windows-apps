using RemodelPilot.Models;
using RemodelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RemodelPilot.Views;

public sealed partial class SubsPage : Page
{
    private readonly JsonListStore<SubEntry> _store = new("subs.json");
    private readonly JsonListStore<Job> _jobStore = new("jobs.json");
    private List<SubEntry> _items = new();
    private List<Job> _jobs = new();
    private SubEntry? _selected;

    public SubsPage()
    {
        this.InitializeComponent();
        WaiverPicker.Date = DateTimeOffset.Now;
        CertPicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _jobs = await _jobStore.LoadAsync();
        JobPicker.ItemsSource = _jobs.Select(j => j.DisplayLine).ToList();
        if (_jobs.Count > 0) JobPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(s => s.AgreedPrice).ToList();
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as SubEntry;
        if (_selected is null) return;
        var idx = _jobs.FindIndex(j => j.Id == _selected.JobId);
        if (idx >= 0) JobPicker.SelectedIndex = idx;
        WhoBox.Text = _selected.Who;
        ScopeBox.Text = _selected.Scope;
        AgreedBox.Text = _selected.AgreedPrice.ToString("0.##");
        PaidBox.Text = _selected.PaidToDate.ToString("0.##");
        WaiverCheck.IsChecked = _selected.WaiverReceived;
        if (_selected.WaiverDate is { } wd) WaiverPicker.Date = wd;
        CertCheck.IsChecked = _selected.CertReceived;
        if (_selected.CertDate is { } cd) CertPicker.Date = cd;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (JobPicker.SelectedIndex < 0 || JobPicker.SelectedIndex >= _jobs.Count) return;
        var job = _jobs[JobPicker.SelectedIndex];
        var item = _selected ?? new SubEntry();
        item.JobId = job.Id;
        item.JobLabel = job.DisplayLine;
        item.Who = WhoBox.Text.Trim();
        item.Scope = ScopeBox.Text.Trim();
        item.AgreedPrice = ParseDec(AgreedBox.Text);
        item.PaidToDate = ParseDec(PaidBox.Text);
        item.WaiverReceived = WaiverCheck.IsChecked == true;
        item.WaiverDate = item.WaiverReceived ? WaiverPicker.Date : null;
        item.CertReceived = CertCheck.IsChecked == true;
        item.CertDate = item.CertReceived ? CertPicker.Date : null;
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(s => s.Id == _selected.Id);
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
        WhoBox.Text = string.Empty;
        ScopeBox.Text = string.Empty;
        AgreedBox.Text = string.Empty;
        PaidBox.Text = string.Empty;
        WaiverCheck.IsChecked = false;
        CertCheck.IsChecked = false;
        NotesBox.Text = string.Empty;
    }
}
