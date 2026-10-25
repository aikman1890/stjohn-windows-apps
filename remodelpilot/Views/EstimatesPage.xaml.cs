using RemodelPilot.Models;
using RemodelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RemodelPilot.Views;

public sealed partial class EstimatesPage : Page
{
    private readonly JsonListStore<EstimateVersion> _store = new("estimates.json");
    private readonly JsonListStore<Job> _jobStore = new("jobs.json");
    private readonly RemodelSettingsStore _settingsStore = new();
    private List<EstimateVersion> _items = new();
    private List<Job> _jobs = new();
    private EstimateVersion? _selected;
    private List<EstimateLine> _editLines = new();

    public EstimatesPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = new[] { "Draft", "Sent", "Won", "Lost" };
        StatusCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        CategoryCombo.ItemsSource = settings.Categories;
        if (settings.Categories.Count > 0) CategoryCombo.SelectedIndex = 0;
        MarkupBox.Text = settings.DefaultMarkupPercent.ToString("0.#");
        _jobs = await _jobStore.LoadAsync();
        JobPicker.ItemsSource = _jobs.Select(j => j.DisplayLine).ToList();
        if (_jobs.Count > 0) JobPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(v => v.Date).ToList();
    }

    private void RefreshLines()
    {
        LinesList.ItemsSource = null;
        LinesList.ItemsSource = _editLines;
        var cost = _editLines.Sum(l => l.Cost);
        var price = _editLines.Sum(l => l.Price);
        TotalsText.Text = $"Lines: cost {cost:C} - price {price:C} - gross margin {price - cost:C}";
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private Job? PickedJob =>
        JobPicker.SelectedIndex >= 0 && JobPicker.SelectedIndex < _jobs.Count
            ? _jobs[JobPicker.SelectedIndex]
            : null;

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as EstimateVersion;
        if (_selected is null) return;
        var idx = _jobs.FindIndex(j => j.Id == _selected.JobId);
        if (idx >= 0) JobPicker.SelectedIndex = idx;
        StatusCombo.SelectedItem = _selected.Status;
        VersionNotesBox.Text = _selected.Notes;
        _editLines = _selected.Lines.Select(l => new EstimateLine
        {
            Id = l.Id, Category = l.Category, Description = l.Description,
            Cost = l.Cost, MarkupPercent = l.MarkupPercent,
        }).ToList();
        RefreshLines();
    }

    private void NewVersionButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        _editLines = new List<EstimateLine>();
        StatusCombo.SelectedIndex = 0;
        VersionNotesBox.Text = string.Empty;
        RefreshLines();
    }

    private void AddLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DescBox.Text)) return;
        _editLines.Add(new EstimateLine
        {
            Category = CategoryCombo.SelectedItem as string ?? "Other",
            Description = DescBox.Text.Trim(),
            Cost = ParseDec(CostBox.Text),
            MarkupPercent = ParseDec(MarkupBox.Text),
        });
        DescBox.Text = string.Empty;
        CostBox.Text = "0";
        RefreshLines();
    }

    private void RemoveLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (LinesList.SelectedItem is EstimateLine line)
        {
            _editLines.Remove(line);
            RefreshLines();
        }
    }

    private async void SaveVersionButton_Click(object sender, RoutedEventArgs e)
    {
        var job = PickedJob;
        if (job is null) return;
        var item = _selected ?? new EstimateVersion
        {
            Version = _items.Where(v => v.JobId == job.Id).Select(v => v.Version).DefaultIfEmpty(0).Max() + 1,
            Date = DateTimeOffset.Now,
        };
        item.JobId = job.Id;
        item.JobLabel = job.DisplayLine;
        item.Status = StatusCombo.SelectedItem as string ?? "Draft";
        item.Notes = VersionNotesBox.Text.Trim();
        item.Lines = _editLines;
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        RefreshLines();
    }

    private async void MarkWonButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.Status = "Won";
        StatusCombo.SelectedItem = "Won";
        _selected.Lines = _editLines;
        await _store.SaveAsync(_items);
        var jobs = await _jobStore.LoadAsync();
        var job = jobs.FirstOrDefault(j => j.Id == _selected.JobId);
        if (job is not null)
        {
            job.BaseContractPrice = _selected.TotalPrice;
            job.WinningVersionId = _selected.Id;
            if (job.Status == "Bid") job.Status = "Won";
            await _jobStore.SaveAsync(jobs);
            _jobs = jobs;
        }
        TotalsText.Text = $"Version {_selected.Version} is the contract: {_selected.TotalPrice:C}. Its category costs are now the job budget.";
        RefreshList();
    }
}
