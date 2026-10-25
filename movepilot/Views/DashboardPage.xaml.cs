using MovePilot.Models;
using MovePilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MovePilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<MoveJob> _moveStore = new("moves.json");
    private readonly JsonListStore<MoveEstimate> _estimateStore = new("estimates.json");
    private readonly JsonListStore<MoveClaim> _claimStore = new("claims.json");
    private readonly JsonListStore<StorageLot> _lotStore = new("lots.json");
    private readonly MoveSettingsStore _settingsStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var moves = await _moveStore.LoadAsync();
        var estimates = await _estimateStore.LoadAsync();
        var claims = await _claimStore.LoadAsync();
        var lots = await _lotStore.LoadAsync();

        CompanyText.Text = string.IsNullOrWhiteSpace(settings.CompanyName)
            ? "Set the company in Settings. The desk core: survey, estimate, approval, job sheet, claim file."
            : settings.CompanyName;

        var today = DateTimeOffset.Now.Date;
        var week = moves.Where(m => m.MoveDate.Date >= today && m.MoveDate.Date <= today.AddDays(7)
                                    && m.Status is not "Closed").ToList();
        var awaiting = estimates.Where(x => x.Status == "Sent").ToList();
        WeekText.Text =
            $"Moves in the next 7 days: {week.Count}" +
            $"{Environment.NewLine}Estimates awaiting approval: {awaiting.Count} ({awaiting.Sum(x => x.Amount):C} unsigned)" +
            $"{Environment.NewLine}Pipeline: {moves.Count(m => m.Status is "Lead" or "Surveyed")} lead(s)/survey(s), {moves.Count(m => m.Status == "Booked")} booked";

        var open = claims.Where(c => c.Status is not "Settled" and not "Declined").ToList();
        ClaimText.Text =
            $"Claims open: {open.Count} - past their acknowledge clock: {open.Count(c => c.AckOverdue)}" +
            $"{Environment.NewLine}Storage balances outstanding: {lots.Where(l => l.OutDate is null).Sum(l => l.Balance):C} across {lots.Count(l => l.OutDate is null)} lot(s)";

        AwaitingList.ItemsSource = awaiting.OrderBy(x => x.CreatedDate).ToList();
    }
}
