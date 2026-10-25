using EntityPilot.Models;
using EntityPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EntityPilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var entities = await new JsonListStore<EntityRecord>("entities.json").LoadAsync();
        var obligations = await new JsonListStore<Obligation>("obligations.json").LoadAsync();
        var inter = await new JsonListStore<IntercompanyEntry>("intercompany.json").LoadAsync();

        PortfolioText.Text = entities.Count == 0
            ? "No entities yet. Add your first one under Entities."
            : $"{entities.Count} entit(ies): {string.Join(", ", entities.GroupBy(x => x.State).Select(g => $"{g.Key} x{g.Count()}"))} - " +
              $"{entities.Count(x => x.Status == "Active")} active, {entities.Count(x => x.Status == "Inactive")} inactive, " +
              $"{entities.Count(x => x.Status == "Dissolved")} dissolved, {entities.Count(x => x.Status == "Unknown")} unconfirmed.";

        var next = obligations.OrderBy(o => o.DueDate).Take(5).ToList();
        DeadlineText.Text = next.Count == 0 ? "No deadlines recorded."
            : string.Join("\n", next.Select(o => o.DisplayLine));

        var lines = new List<string>();
        foreach (var o in obligations.Where(o => o.DaysUntil < 0).OrderBy(o => o.DueDate))
            lines.Add($"OVERDUE: {o.EntityLabel} - {o.Title} ({-o.DaysUntil} days).");
        foreach (var x in entities.Where(x => x.AgentDays is < 0))
            lines.Add($"AGENT RENEWAL OVERDUE: {x.LegalName}.");
        foreach (var x in entities.Where(x => x.Status == "Unknown"))
            lines.Add($"STATUS NEVER CONFIRMED: {x.LegalName} - check the state portal and record the date.");
        foreach (var x in entities.Where(x => x.StatusConfirmedDate is { } c && (DateTimeOffset.Now - c).TotalDays > 365))
            lines.Add($"Status confirmation is over a year old: {x.LegalName}.");

        var owed = inter.Where(i => i.Kind != "Repayment").Sum(i => i.Amount)
                 - inter.Where(i => i.Kind == "Repayment").Sum(i => i.Amount);
        lines.Add($"Intercompany balances outstanding (all pairs): {owed:C}.");
        AttentionText.Text = string.Join("\n", lines);
    }
}
