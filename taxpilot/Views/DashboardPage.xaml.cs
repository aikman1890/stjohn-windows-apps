using TaxPilot.Models;
using TaxPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TaxPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<Engagement> _engStore = new("engagements.json");
    private readonly JsonListStore<DocItem> _docStore = new("docs.json");
    private readonly TaxSettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var engagements = (await _engStore.LoadAsync()).Where(x => x.TaxYear == settings.CurrentTaxYear).ToList();
        DeadlineCalc.ApplyDueDates(engagements, settings);
        var docs = await _docStore.LoadAsync();

        OfficeText.Text = string.IsNullOrWhiteSpace(settings.OfficeName)
            ? $"Tax year {settings.CurrentTaxYear} season. Set the office in Settings. Your client list never leaves your office."
            : $"{settings.OfficeName} - tax year {settings.CurrentTaxYear}";

        var byStatus = engagements.GroupBy(x => x.Status).OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key}: {g.Count()}");
        PipelineText.Text = string.Join(Environment.NewLine, byStatus);

        int DueIn(int days) => engagements.Count(x => x.Status is not "Accepted" and not "Done" &&
            x.DaysToDue is { } d && d >= 0 && d <= days);
        var engIds = engagements.Select(x => x.Id).ToHashSet();
        ChaseText.Text =
            $"Due in 7 days: {DueIn(7)} - 14 days: {DueIn(14)} - 30 days: {DueIn(30)} - overdue: {engagements.Count(x => x.Status is not "Accepted" and not "Done" && x.DaysToDue is < 0)}" +
            $"{Environment.NewLine}Documents outstanding: {docs.Count(d => engIds.Contains(d.EngagementId) && !d.Received)}" +
            $"{Environment.NewLine}Extensions to file: {engagements.Count(x => x.Status == "Extended" && !x.Extended)}" +
            $"{Environment.NewLine}Fees outstanding: {engagements.Sum(x => x.FeeOwed):C}";

        DueList.ItemsSource = engagements
            .Where(x => x.Status is not "Accepted" and not "Done" && x.ComputedDue is not null)
            .OrderBy(x => x.ComputedDue).Take(10).ToList();
    }
}
