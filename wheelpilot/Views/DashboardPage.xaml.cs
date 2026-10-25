using WheelPilot.Models;
using WheelPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WheelPilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await new WheelSettingsStore().LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.SchoolName))
            TitleText.Text = settings.SchoolName;

        var students = await new JsonListStore<Student>("students.json").LoadAsync();
        var lessons = await new JsonListStore<Lesson>("lessons.json").LoadAsync();
        var purchases = await new JsonListStore<Purchase>("purchases.json").LoadAsync();
        var cars = await new JsonListStore<Car>("cars.json").LoadAsync();
        var today = DateTimeOffset.Now.Date;

        var todays = lessons.Where(l => l.Date.Date == today && l.Status == "Scheduled")
            .OrderBy(l => l.StartHour).ToList();
        TodayText.Text = todays.Count == 0 ? "No lessons today."
            : string.Join("\n", todays.Select(l => $"{l.StartHour:0.0}-{l.EndHour:0.0}h - {l.StudentLabel} with {l.InstructorLabel} in {l.CarLabel}"));

        var sold = purchases.Sum(p => p.Hours);
        var delivered = lessons.Where(l => l.Status == "Completed").Sum(l => l.DurationHours);
        var owed = purchases.Sum(p => p.Owed);
        var monthRevenue = purchases
            .Where(p => p.Date.ToString("yyyy-MM") == DateTimeOffset.Now.ToString("yyyy-MM"))
            .Sum(p => p.PaidAmount);
        MoneyText.Text = $"Package hours sold: {sold:0.0}h - delivered: {delivered:0.0}h. Balances owing on packages: {owed:C}. Payments recorded this month: {monthRevenue:C}.";

        var lines = new List<string>();
        var tests = students.Where(s => s.TestDate is { } t && t.Date >= today && (t.Date - today).Days <= 7)
            .OrderBy(s => s.TestDate).ToList();
        foreach (var s in tests) lines.Add($"Road test in {(s.TestDate!.Value.Date - today).Days} day(s): {s.Name} ({s.TestDate:ddd, MMM d}).");
        foreach (var s in students.Where(s => s.InquiryStale))
            lines.Add($"INQUIRY STALE over 5 days: {s.Name} (source: {s.Source}).");
        foreach (var s in students.Where(s => s.PermitDays is < 0))
            lines.Add($"PERMIT EXPIRED on file: {s.Name}.");
        foreach (var c in cars.Where(c => c.OutOfService))
            lines.Add($"CAR OUT OF SERVICE: {c.Name} - {c.OutOfServiceNote}");
        foreach (var c in cars.Where(c => c.NearestExpiry is { } x && (x.Date - today).Days <= 30))
            lines.Add($"Car date expires in {(c.NearestExpiry!.Value.Date - today).Days} day(s): {c.Name}.");
        AttentionText.Text = lines.Count == 0 ? "Nothing flagged. Clean board." : string.Join("\n", lines);
    }
}
