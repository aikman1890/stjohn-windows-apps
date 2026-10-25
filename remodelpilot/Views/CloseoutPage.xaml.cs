using RemodelPilot.Models;
using RemodelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RemodelPilot.Views;

public sealed partial class CloseoutPage : Page
{
    private readonly JsonListStore<PunchItem> _punchStore = new("punch.json");
    private readonly JsonListStore<WarrantyCallback> _callbackStore = new("callbacks.json");
    private readonly JsonListStore<Job> _jobStore = new("jobs.json");
    private List<Job> _jobs = new();
    private List<PunchItem> _punch = new();
    private List<WarrantyCallback> _callbacks = new();

    public CloseoutPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _jobs = await _jobStore.LoadAsync();
        JobPicker.ItemsSource = _jobs.Select(j => j.DisplayLine).ToList();
        if (_jobs.Count > 0) JobPicker.SelectedIndex = 0;
        _punch = await _punchStore.LoadAsync();
        _callbacks = await _callbackStore.LoadAsync();
        RefreshAll();
    }

    private Job? PickedJob =>
        JobPicker.SelectedIndex >= 0 && JobPicker.SelectedIndex < _jobs.Count
            ? _jobs[JobPicker.SelectedIndex]
            : null;

    private void JobPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshAll();

    private void RefreshAll()
    {
        var job = PickedJob;
        PunchList.ItemsSource = job is null ? null : _punch.Where(p => p.JobId == job.Id).ToList();
        WarrantyText.Text = job is null
            ? string.Empty
            : job.WarrantyEnd is { } w
                ? $"{job.ClientName}: warranty {(w.Date < DateTimeOffset.Now.Date ? "EXPIRED" : "runs")} through {w:MMM d, yyyy}. Callbacks on this job: {_callbacks.Count(c => c.JobId == job.Id)}."
                : $"{job.ClientName}: no warranty end date set - set it on the Jobs page when the job completes.";
        CallbackList.ItemsSource = _callbacks.OrderByDescending(c => c.Date).ToList();
    }

    private async void AddPunchButton_Click(object sender, RoutedEventArgs e)
    {
        var job = PickedJob;
        if (job is null || string.IsNullOrWhiteSpace(PunchBox.Text)) return;
        _punch.Add(new PunchItem { JobId = job.Id, Description = PunchBox.Text.Trim() });
        await _punchStore.SaveAsync(_punch);
        PunchBox.Text = string.Empty;
        RefreshAll();
    }

    private async void DonePunchButton_Click(object sender, RoutedEventArgs e)
    {
        if (PunchList.SelectedItem is not PunchItem item) return;
        item.Done = true;
        item.DoneDate = DateTimeOffset.Now;
        await _punchStore.SaveAsync(_punch);
        RefreshAll();
    }

    private async void DeletePunchButton_Click(object sender, RoutedEventArgs e)
    {
        if (PunchList.SelectedItem is not PunchItem item) return;
        _punch.RemoveAll(p => p.Id == item.Id);
        await _punchStore.SaveAsync(_punch);
        RefreshAll();
    }

    private async void AddCallbackButton_Click(object sender, RoutedEventArgs e)
    {
        var job = PickedJob;
        if (job is null || string.IsNullOrWhiteSpace(IssueBox.Text)) return;
        _callbacks.Add(new WarrantyCallback
        {
            JobId = job.Id,
            JobLabel = job.DisplayLine,
            Date = DateTimeOffset.Now,
            Issue = IssueBox.Text.Trim(),
            Resolution = ResolutionBox.Text.Trim(),
            Cost = decimal.TryParse(CallbackCostBox.Text.Trim().TrimStart('$'), out var v) ? v : 0m,
        });
        await _callbackStore.SaveAsync(_callbacks);
        IssueBox.Text = string.Empty;
        ResolutionBox.Text = string.Empty;
        CallbackCostBox.Text = "0";
        RefreshAll();
    }

    private async void DeleteCallbackButton_Click(object sender, RoutedEventArgs e)
    {
        if (CallbackList.SelectedItem is not WarrantyCallback cb) return;
        _callbacks.RemoveAll(c => c.Id == cb.Id);
        await _callbackStore.SaveAsync(_callbacks);
        RefreshAll();
    }
}
