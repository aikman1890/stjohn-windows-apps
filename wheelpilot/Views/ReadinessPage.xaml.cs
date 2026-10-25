using WheelPilot.Models;
using WheelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WheelPilot.Views;

public sealed partial class ReadinessPage : Page
{
    private readonly JsonListStore<Student> _studentStore = new("students.json");
    private readonly JsonListStore<MilestoneMark> _markStore = new("milestones.json");
    private readonly JsonListStore<AttendanceEntry> _attStore = new("attendance.json");
    private readonly WheelSettingsStore _settingsStore = new();
    private List<Student> _students = new();
    private WheelSettings _settings = new();
    private Student? _current;

    public ReadinessPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _settings = await _settingsStore.LoadAsync();
        _students = (await _studentStore.LoadAsync()).OrderBy(s => s.Name).ToList();
        StudentBox.ItemsSource = _students.Select(s => s.Name).ToList();
        SkillBox.ItemsSource = _settings.Skills;
        var sessions = new List<string>();
        foreach (var line in _settings.Courses)
        {
            var parts = line.Split('|');
            if (parts.Length < 2) continue;
            for (var i = 1; i < parts.Length; i++) sessions.Add($"{parts[0]} / {parts[i]}");
        }
        SessionBox.ItemsSource = sessions;

        TestList.ItemsSource = _students
            .Where(s => s.TestDate is not null)
            .OrderBy(s => s.TestDate)
            .Select(s => $"{s.Name} - test {s.TestDate:ddd, MMM d, yyyy} - {s.Status} - {s.TestNote}")
            .ToList();
        if (_students.Count > 0) { StudentBox.SelectedIndex = 0; await LoadRecordAsync(); }
    }

    private async void LoadButton_Click(object sender, RoutedEventArgs e) => await LoadRecordAsync();

    private async Task LoadRecordAsync()
    {
        var idx = StudentBox.SelectedIndex;
        _current = idx >= 0 && idx < _students.Count ? _students[idx] : null;
        if (_current is null) return;
        var marks = await _markStore.LoadAsync();
        var mine = marks.Where(m => m.StudentId == _current.Id).ToList();
        SkillList.ItemsSource = _settings.Skills.Select(sk =>
        {
            var m = mine.FirstOrDefault(x => x.Skill == sk);
            return m is null ? $"[ ] {sk}" : $"[x] {sk} - {m.Date:MMM d} - signed {m.SignedBy}";
        }).ToList();
        var total = _settings.Skills.Count;
        SummaryText.Text = $"{_current.Name}: {mine.Count} of {total} milestones signed off" +
            (_current.TestDate is { } t ? $" - road test {t:ddd, MMM d, yyyy}" : " - no test date booked") +
            $" - status {_current.Status}.";

        var att = await _attStore.LoadAsync();
        AttList.ItemsSource = att.Where(a => a.StudentId == _current.Id)
            .OrderBy(a => a.Date).Select(a => a.DisplayLine).ToList();
    }

    private async void MarkButton_Click(object sender, RoutedEventArgs e)
    {
        if (_current is null || SkillBox.SelectedItem is not string skill)
        { ErrorText.Text = "Load a student and pick a milestone."; return; }
        var marks = await _markStore.LoadAsync();
        marks.RemoveAll(m => m.StudentId == _current.Id && m.Skill == skill);
        marks.Add(new MilestoneMark
        {
            StudentId = _current.Id, Skill = skill, Date = DateTimeOffset.Now,
            SignedBy = string.IsNullOrWhiteSpace(SignerBox.Text) ? "Instructor" : SignerBox.Text.Trim(),
        });
        await _markStore.SaveAsync(marks);
        await LoadRecordAsync();
    }

    private async void UnmarkButton_Click(object sender, RoutedEventArgs e)
    {
        if (_current is null || SkillBox.SelectedItem is not string skill) return;
        var marks = await _markStore.LoadAsync();
        marks.RemoveAll(m => m.StudentId == _current.Id && m.Skill == skill);
        await _markStore.SaveAsync(marks);
        await LoadRecordAsync();
    }

    private async void AttButton_Click(object sender, RoutedEventArgs e)
    {
        if (_current is null || SessionBox.SelectedItem is not string session)
        { ErrorText.Text = "Load a student and pick a session."; return; }
        var split = session.Split(" / ", 2);
        var att = await _attStore.LoadAsync();
        att.Add(new AttendanceEntry
        {
            StudentId = _current.Id, StudentLabel = _current.Name,
            Course = split.ElementAtOrDefault(0) ?? string.Empty,
            SessionLabel = split.ElementAtOrDefault(1) ?? session,
            Date = DateTimeOffset.Now,
        });
        await _attStore.SaveAsync(att);
        await LoadRecordAsync();
    }
}
