using System.Globalization;
using WheelPilot.Models;
using WheelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WheelPilot.Views;

public sealed partial class SchedulePage : Page
{
    private readonly JsonListStore<Lesson> _store = new("lessons.json");
    private readonly JsonListStore<Student> _studentStore = new("students.json");
    private readonly JsonListStore<Instructor> _instructorStore = new("instructors.json");
    private readonly JsonListStore<Car> _carStore = new("cars.json");
    private List<Lesson> _lessons = new();
    private List<Student> _students = new();
    private List<Instructor> _instructors = new();
    private List<Car> _cars = new();
    private Lesson? _selected;
    private DateTimeOffset _weekStart;

    public SchedulePage()
    {
        this.InitializeComponent();
        var today = DateTimeOffset.Now.Date;
        var delta = ((int)today.DayOfWeek + 6) % 7;
        _weekStart = new DateTimeOffset(today.AddDays(-delta), TimeSpan.Zero);
        StatusBox.ItemsSource = new[] { "Scheduled", "Completed", "Cancelled", "No-show" };
        StudentBox.SelectionChanged += async (_, _) => await ShowBalanceHintAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _students = (await _studentStore.LoadAsync()).OrderBy(s => s.Name).ToList();
        _instructors = (await _instructorStore.LoadAsync()).Where(i => i.Active).OrderBy(i => i.Name).ToList();
        _cars = (await _carStore.LoadAsync()).OrderBy(c => c.Name).ToList();
        StudentBox.ItemsSource = _students.Select(s => s.Name).ToList();
        InstructorBox.ItemsSource = _instructors.Select(i => i.Name).ToList();
        CarBox.ItemsSource = _cars.Select(c => c.Name).ToList();
        _lessons = await _store.LoadAsync();
        Refresh();
    }

    private void Refresh()
    {
        WeekText.Text = $"{_weekStart:MMM d} - {_weekStart.AddDays(6):MMM d}";
        var week = _lessons
            .Where(l => l.Date.Date >= _weekStart.Date && l.Date.Date <= _weekStart.AddDays(6).Date)
            .OrderBy(l => l.Date).ThenBy(l => l.StartHour).ToList();
        LessonList.ItemsSource = week;

        var problems = new List<string>();
        foreach (var l in week)
            problems.AddRange(ConflictCheck.FindConflicts(l, _lessons, _cars)
                .Select(p => $"{l.Date:ddd} {l.StartHour:0.0}h: {p}"));
        ProblemList.ItemsSource = problems.Distinct().ToList();
        if (problems.Count == 0)
            ProblemList.ItemsSource = new List<string> { "No conflicts in the visible week." };
    }

    private async Task ShowBalanceHintAsync()
    {
        if (StudentBox.SelectedIndex < 0 || StudentBox.SelectedIndex >= _students.Count)
        { BalanceHintText.Text = string.Empty; return; }
        var s = _students[StudentBox.SelectedIndex];
        var purchases = await new JsonListStore<Purchase>("purchases.json").LoadAsync();
        var adjs = await new JsonListStore<Adjustment>("adjustments.json").LoadAsync();
        BalanceHintText.Text = $"Balance for {s.Name}: {BalanceCalc.HoursRemaining(s.Id, purchases, _lessons, adjs):0.0}h remaining.";
    }

    private void LessonList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = LessonList.SelectedItem as Lesson;
        if (_selected is null) return;
        StudentBox.SelectedItem = _selected.StudentLabel;
        InstructorBox.SelectedItem = _selected.InstructorLabel;
        CarBox.SelectedItem = _selected.CarLabel;
        StatusBox.SelectedItem = _selected.Status;
        DatePicker.Date = _selected.Date;
        StartBox.Text = _selected.StartHour.ToString(CultureInfo.InvariantCulture);
        DurationBox.Text = _selected.DurationHours.ToString(CultureInfo.InvariantCulture);
        PickupBox.Text = _selected.PickupLocation;
        SkillsBox.Text = _selected.SkillsCovered;
        NotesBox.Text = _selected.Notes;
        NextBox.Text = _selected.NextRecommendation;
        ErrorText.Text = string.Empty;
        _ = ShowBalanceHintAsync();
    }

    private void PrevButton_Click(object sender, RoutedEventArgs e) { _weekStart = _weekStart.AddDays(-7); Refresh(); }
    private void NextButton_Click(object sender, RoutedEventArgs e) { _weekStart = _weekStart.AddDays(7); Refresh(); }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (StudentBox.SelectedIndex < 0) { ErrorText.Text = "Pick a student."; return; }
        var l = _selected ?? new Lesson();
        var si = StudentBox.SelectedIndex;
        var ii = InstructorBox.SelectedIndex;
        var ci = CarBox.SelectedIndex;
        l.StudentId = _students[si].Id; l.StudentLabel = _students[si].Name;
        l.InstructorId = ii >= 0 && ii < _instructors.Count ? _instructors[ii].Id : string.Empty;
        l.InstructorLabel = ii >= 0 && ii < _instructors.Count ? _instructors[ii].Name : string.Empty;
        l.CarId = ci >= 0 && ci < _cars.Count ? _cars[ci].Id : string.Empty;
        l.CarLabel = ci >= 0 && ci < _cars.Count ? _cars[ci].Name : string.Empty;
        l.Status = StatusBox.SelectedItem as string ?? l.Status;
        l.Date = DatePicker.Date ?? DateTimeOffset.Now;
        if (decimal.TryParse(StartBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var st)) l.StartHour = st;
        if (decimal.TryParse(DurationBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var du) && du > 0) l.DurationHours = du;
        l.PickupLocation = PickupBox.Text.Trim();
        l.SkillsCovered = SkillsBox.Text.Trim();
        l.Notes = NotesBox.Text.Trim();
        l.NextRecommendation = NextBox.Text.Trim();

        var conflicts = ConflictCheck.FindConflicts(l, _lessons, _cars);
        if (conflicts.Count > 0)
        {
            ErrorText.Text = "Not saved - conflict:\n" + string.Join("\n", conflicts);
            return;
        }
        if (_selected is null) _lessons.Add(l);
        await _store.SaveAsync(_lessons);
        Refresh();
        LessonList.SelectedItem = _lessons.FirstOrDefault(x => x.Id == l.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _lessons.Remove(_selected);
        await _store.SaveAsync(_lessons);
        _selected = null;
        Refresh();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        LessonList.SelectedItem = null;
        DatePicker.Date = DateTimeOffset.Now;
        StartBox.Text = "15";
        DurationBox.Text = "1.5";
        StatusBox.SelectedItem = "Scheduled";
        PickupBox.Text = SkillsBox.Text = NotesBox.Text = NextBox.Text = string.Empty;
        ErrorText.Text = string.Empty;
    }
}
