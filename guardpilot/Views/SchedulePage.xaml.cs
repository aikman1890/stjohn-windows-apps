using System.Globalization;
using GuardPilot.Models;
using GuardPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GuardPilot.Views;

public sealed partial class SchedulePage : Page
{
    private readonly JsonListStore<Shift> _store = new("shifts.json");
    private readonly JsonListStore<Post> _postStore = new("posts.json");
    private readonly JsonListStore<Guard> _guardStore = new("guards.json");
    private readonly JsonListStore<ClientSite> _siteStore = new("sites.json");
    private readonly GuardSettingsStore _settingsStore = new();
    private List<Shift> _items = new();
    private List<Post> _posts = new();
    private List<Guard> _guards = new();
    private List<ClientSite> _sites = new();
    private Shift? _selected;
    private DateTimeOffset _weekStart;

    public SchedulePage()
    {
        this.InitializeComponent();
        var today = DateTimeOffset.Now.Date;
        _weekStart = today.AddDays(-(((int)DateTimeOffset.Now.DayOfWeek + 6) % 7));
        DatePickerCtl.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _posts = await _postStore.LoadAsync();
        PostPicker.ItemsSource = _posts.Select(p => p.DisplayLine).ToList();
        _guards = await _guardStore.LoadAsync();
        GuardPicker.ItemsSource = new[] { "(unfilled)" }.Concat(_guards.Where(g => g.Status == "Active").Select(g => g.Name)).ToList();
        GuardPicker.SelectedIndex = 0;
        _sites = await _siteStore.LoadAsync();
        if (_posts.Count > 0) PostPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        var week = _items.Where(s => s.Date.Date >= _weekStart && s.Date.Date < _weekStart.AddDays(7))
            .OrderBy(s => s.Date).ThenBy(s => s.StartHour).ToList();
        ItemsList.ItemsSource = week;
        WeekText.Text = $"Week of {_weekStart:MMM d, yyyy} - {week.Count} shift(s), {week.Sum(s => s.ScheduledHours):0.0} scheduled hours";
        _ = RefreshProblemsAsync(week);
    }

    private async Task RefreshProblemsAsync(List<Shift> week)
    {
        var settings = await _settingsStore.LoadAsync();
        ProblemList.ItemsSource = ScheduleCheck.FindProblems(week, _guards, settings.OvertimeWeeklyThreshold);
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Shift;
        if (_selected is null) return;
        var pIdx = _posts.FindIndex(p => p.Id == _selected.PostId);
        if (pIdx >= 0) PostPicker.SelectedIndex = pIdx;
        DatePickerCtl.Date = _selected.Date;
        StartBox.Text = _selected.StartHour.ToString(CultureInfo.InvariantCulture);
        EndBox.Text = _selected.EndHour.ToString(CultureInfo.InvariantCulture);
        var activeGuards = _guards.Where(g => g.Status == "Active").ToList();
        var gIdx = activeGuards.FindIndex(g => g.Id == _selected.GuardId);
        GuardPicker.SelectedIndex = gIdx >= 0 ? gIdx + 1 : 0;
        ActualBox.Text = _selected.ActualHours?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        NotesBox.Text = _selected.Notes;
    }

    private decimal ParseNum(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (PostPicker.SelectedIndex < 0 || PostPicker.SelectedIndex >= _posts.Count) return;
        var post = _posts[PostPicker.SelectedIndex];
        var site = _sites.FirstOrDefault(s => s.Id == post.SiteId);
        var item = _selected ?? new Shift();
        item.PostId = post.Id;
        item.PostLabel = post.DisplayLine;
        item.SiteLabel = post.SiteLabel;
        item.BillRatePerHour = site?.BillRatePerHour ?? item.BillRatePerHour;
        item.Date = DatePickerCtl.Date ?? DateTimeOffset.Now;
        item.StartHour = ParseNum(StartBox.Text);
        item.EndHour = ParseNum(EndBox.Text);
        var activeGuards = _guards.Where(g => g.Status == "Active").ToList();
        if (GuardPicker.SelectedIndex > 0 && GuardPicker.SelectedIndex - 1 < activeGuards.Count)
        {
            var guard = activeGuards[GuardPicker.SelectedIndex - 1];
            item.GuardId = guard.Id;
            item.GuardLabel = guard.Name;
        }
        else
        {
            item.GuardId = string.Empty;
            item.GuardLabel = string.Empty;
        }
        item.ActualHours = string.IsNullOrWhiteSpace(ActualBox.Text) ? null : ParseNum(ActualBox.Text);
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
        ActualBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }

    private void PrevWeekButton_Click(object sender, RoutedEventArgs e)
    {
        _weekStart = _weekStart.AddDays(-7);
        RefreshList();
    }

    private void NextWeekButton_Click(object sender, RoutedEventArgs e)
    {
        _weekStart = _weekStart.AddDays(7);
        RefreshList();
    }
}
