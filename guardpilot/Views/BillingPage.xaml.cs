using System.Net;
using System.Text;
using GuardPilot.Models;
using GuardPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GuardPilot.Views;

public sealed partial class BillingPage : Page
{
    private readonly JsonListStore<Shift> _shiftStore = new("shifts.json");
    private readonly GuardSettingsStore _settingsStore = new();
    private List<string> _siteLines = new();
    private List<string> _guardLines = new();
    private string _month = string.Empty;

    public BillingPage()
    {
        this.InitializeComponent();
        MonthBox.Text = DateTimeOffset.Now.ToString("yyyy-MM");
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _ = CalculateAsync();
    }

    private async Task CalculateAsync()
    {
        _month = MonthBox.Text.Trim();
        var shifts = (await _shiftStore.LoadAsync())
            .Where(s => s.Date.ToString("yyyy-MM") == _month).ToList();

        _siteLines = shifts.GroupBy(s => s.SiteLabel).OrderBy(g => g.Key).Select(g =>
        {
            var sched = g.Sum(s => s.ScheduledHours);
            var actual = g.Where(s => s.ActualHours is not null).Sum(s => s.ActualHours ?? 0m);
            var billable = g.Where(s => s.ActualHours is not null).Sum(s => (s.ActualHours ?? 0m) * s.BillRatePerHour);
            var variance = actual - sched;
            return $"{g.Key}: scheduled {sched:0.0}h - actual {actual:0.0}h (variance {variance:+0.0;-0.0;0.0}h) - billable on actuals {billable:C} - {g.Count(s => s.ActualHours is null)} shift(s) missing actuals";
        }).ToList();
        SiteList.ItemsSource = _siteLines;

        _guardLines = shifts.Where(s => !s.Unfilled).GroupBy(s => s.GuardLabel).OrderBy(g => g.Key).Select(g =>
        {
            var sched = g.Sum(s => s.ScheduledHours);
            var actual = g.Where(s => s.ActualHours is not null).Sum(s => s.ActualHours ?? 0m);
            return $"{g.Key}: scheduled {sched:0.0}h - actual {actual:0.0}h";
        }).ToList();
        GuardList.ItemsSource = _guardLines;

        TotalsText.Text = shifts.Count == 0
            ? $"No shifts recorded in {_month}."
            : $"{_month}: {shifts.Count} shift(s) - scheduled {shifts.Sum(s => s.ScheduledHours):0.0}h - actual {shifts.Where(s => s.ActualHours is not null).Sum(s => s.ActualHours ?? 0m):0.0}h";
    }

    private async void CalcButton_Click(object sender, RoutedEventArgs e) => await CalculateAsync();

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        await CalculateAsync();
        var settings = await _settingsStore.LoadAsync();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Hours summary</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:21px}h2{font-size:15px;margin-top:18px}.muted{color:#666}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.CompanyName)} - Verified hours summary, {_month}</h1>");
        sb.AppendLine("<h2>Per site</h2>");
        foreach (var line in _siteLines) sb.AppendLine($"<p>{Esc(line)}</p>");
        sb.AppendLine("<h2>Guard pay hours</h2>");
        foreach (var line in _guardLines) sb.AppendLine($"<p>{Esc(line)}</p>");
        sb.AppendLine("<p class=\"muted\">Produced by GuardPilot from scheduled shifts and office-entered actuals. Invoice from these verified hours in your invoicing tool.</p>");
        sb.AppendLine("</body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = $"hours-summary-{_month}.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Summary saved to {file.Path}";
    }
}
