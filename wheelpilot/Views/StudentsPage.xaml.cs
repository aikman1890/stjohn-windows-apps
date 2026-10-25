using System.Globalization;
using WheelPilot.Models;
using WheelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WheelPilot.Views;

public sealed partial class StudentsPage : Page
{
    private readonly JsonListStore<Student> _store = new("students.json");
    private readonly JsonListStore<Purchase> _purchaseStore = new("purchases.json");
    private readonly JsonListStore<Lesson> _lessonStore = new("lessons.json");
    private readonly JsonListStore<Adjustment> _adjStore = new("adjustments.json");
    private readonly WheelSettingsStore _settingsStore = new();
    private List<Student> _students = new();
    private Student? _selected;
    private List<(string Name, decimal Hours, decimal Price)> _packages = new();

    public StudentsPage()
    {
        this.InitializeComponent();
        StatusBox.ItemsSource = new[] { "Inquiry", "Enrolled", "In training", "Test-ready", "Passed", "Completed", "Inactive" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        _packages = settings.Packages.Select(p =>
        {
            var parts = p.Split('|');
            return (parts.ElementAtOrDefault(0) ?? p,
                decimal.TryParse(parts.ElementAtOrDefault(1), NumberStyles.Any, CultureInfo.InvariantCulture, out var h) ? h : 0m,
                decimal.TryParse(parts.ElementAtOrDefault(2), NumberStyles.Any, CultureInfo.InvariantCulture, out var pr) ? pr : 0m);
        }).ToList();
        PackageBox.ItemsSource = _packages.Select(p => $"{p.Name} - {p.Hours:0.0}h - {p.Price:C}").ToList();
        await ReloadAsync();
    }

    private async Task ReloadAsync(string? keepId = null)
    {
        _students = (await _store.LoadAsync()).OrderBy(s => s.Name).ToList();
        StudentList.ItemsSource = _students;
        if (keepId is not null)
            StudentList.SelectedItem = _students.FirstOrDefault(s => s.Id == keepId);
        else if (_students.Count > 0)
            StudentList.SelectedIndex = 0;
    }

    private void StudentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = StudentList.SelectedItem as Student;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        StatusBox.SelectedItem = _selected.Status;
        PhoneBox.Text = _selected.Phone;
        EmailBox.Text = _selected.Email;
        ParentBox.Text = _selected.ParentName;
        ParentPhoneBox.Text = _selected.ParentPhone;
        PermitBox.Text = _selected.PermitNumber;
        PermitPicker.Date = _selected.PermitExpiry;
        CourseBox.Text = _selected.Course;
        SourceBox.Text = _selected.Source;
        TestPicker.Date = _selected.TestDate;
        TestNoteBox.Text = _selected.TestNote;
        NotesBox.Text = _selected.Notes;
        ErrorText.Text = string.Empty;
        _ = RefreshBalanceAsync();
    }

    private async Task RefreshBalanceAsync()
    {
        if (_selected is null) { BalanceText.Text = string.Empty; HistoryList.ItemsSource = null; return; }
        var purchases = await _purchaseStore.LoadAsync();
        var lessons = await _lessonStore.LoadAsync();
        var adjs = await _adjStore.LoadAsync();
        var remaining = BalanceCalc.HoursRemaining(_selected.Id, purchases, lessons, adjs);
        var owed = BalanceCalc.MoneyOwed(_selected.Id, purchases);
        BalanceText.Text = $"Hours remaining: {remaining:0.0}h" + (owed > 0 ? $" - OWES {owed:C} on packages" : " - paid in full");

        var lines = new List<(DateTimeOffset When, string Line)>();
        foreach (var p in purchases.Where(p => p.StudentId == _selected.Id))
            lines.Add((p.Date, $"+{p.Hours:0.0}h purchased - {p.PackageName} ({p.Price:C}, paid {p.PaidAmount:C})"));
        foreach (var l in lessons.Where(l => l.StudentId == _selected.Id && l.Status == "Completed").OrderBy(l => l.Date))
            lines.Add((l.Date, $"-{l.DurationHours:0.0}h lesson {l.Date:MMM d} with {l.InstructorLabel}"));
        foreach (var a in adjs.Where(a => a.StudentId == _selected.Id))
            lines.Add((a.Date, $"{a.HoursDelta:+0.0;-0.0}h adjustment - {a.Reason}"));
        HistoryList.ItemsSource = lines.OrderBy(x => x.When).Select(x => x.Line).ToList();
    }

    private void ReadForm(Student s)
    {
        s.Name = NameBox.Text.Trim();
        s.Status = StatusBox.SelectedItem as string ?? s.Status;
        s.Phone = PhoneBox.Text.Trim();
        s.Email = EmailBox.Text.Trim();
        s.ParentName = ParentBox.Text.Trim();
        s.ParentPhone = ParentPhoneBox.Text.Trim();
        s.PermitNumber = PermitBox.Text.Trim();
        s.PermitExpiry = PermitPicker.Date;
        s.Course = CourseBox.Text.Trim();
        s.Source = SourceBox.Text.Trim();
        s.TestDate = TestPicker.Date;
        s.TestNote = TestNoteBox.Text.Trim();
        s.Notes = NotesBox.Text.Trim();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if ((_selected is null && string.IsNullOrWhiteSpace(NameBox.Text)) ||
            (_selected is not null && string.IsNullOrWhiteSpace(NameBox.Text)))
        { ErrorText.Text = "A name is required."; return; }
        var s = _selected ?? new Student();
        ReadForm(s);
        if (_selected is null) _students.Add(s);
        await _store.SaveAsync(_students);
        await ReloadAsync(s.Id);
    }

    private async void TouchButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.LastContacted = DateTimeOffset.Now;
        await _store.SaveAsync(_students);
        await ReloadAsync(_selected.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _students.Remove(_selected);
        await _store.SaveAsync(_students);
        _selected = null;
        await ReloadAsync();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        StudentList.SelectedItem = null;
        NameBox.Text = PhoneBox.Text = EmailBox.Text = ParentBox.Text = ParentPhoneBox.Text =
            PermitBox.Text = CourseBox.Text = SourceBox.Text = TestNoteBox.Text = NotesBox.Text = string.Empty;
        StatusBox.SelectedItem = "Enrolled";
        PermitPicker.Date = null;
        TestPicker.Date = null;
        BalanceText.Text = string.Empty;
        HistoryList.ItemsSource = null;
    }

    private async void BuyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select a student first."; return; }
        if (PackageBox.SelectedIndex < 0 || PackageBox.SelectedIndex >= _packages.Count)
        { ErrorText.Text = "Pick a package."; return; }
        var pkg = _packages[PackageBox.SelectedIndex];
        var paid = decimal.TryParse(PaidBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var pv) ? pv : pkg.Price;
        var purchases = await _purchaseStore.LoadAsync();
        purchases.Add(new Purchase
        {
            StudentId = _selected.Id, StudentLabel = _selected.Name,
            PackageName = pkg.Name, Hours = pkg.Hours, Price = pkg.Price, PaidAmount = paid,
            Method = MethodBox.Text.Trim(), Date = DateTimeOffset.Now,
        });
        await _purchaseStore.SaveAsync(purchases);
        PaidBox.Text = MethodBox.Text = string.Empty;
        await RefreshBalanceAsync();
        await ReloadAsync(_selected.Id);
    }

    private async void AdjButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select a student first."; return; }
        if (!decimal.TryParse(AdjHoursBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var delta) ||
            string.IsNullOrWhiteSpace(AdjReasonBox.Text))
        { ErrorText.Text = "An adjustment needs hours and a reason - corrections are dated entries, never silent edits."; return; }
        var adjs = await _adjStore.LoadAsync();
        adjs.Add(new Adjustment
        {
            StudentId = _selected.Id, StudentLabel = _selected.Name,
            HoursDelta = delta, Reason = AdjReasonBox.Text.Trim(), Date = DateTimeOffset.Now,
        });
        await _adjStore.SaveAsync(adjs);
        AdjHoursBox.Text = AdjReasonBox.Text = string.Empty;
        await RefreshBalanceAsync();
    }
}
