using CasePilot.Models;
using CasePilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CasePilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<CaseItem> _store = new("cases.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var items = await _store.LoadAsync();
        TotalText.Text = items.Count.ToString();
        ActiveText.Text = items.Count(c => !c.IsClosed).ToString();
        DueSoonText.Text = items.Count(c => !c.IsClosed && c.DaysRemaining >= 0 && c.DaysRemaining <= 5).ToString();
        OverdueText.Text = items.Count(c => !c.IsClosed && c.DaysRemaining < 0).ToString();
        DeadlineList.ItemsSource = items
            .Where(c => !c.IsClosed)
            .OrderBy(c => c.NextDeadline)
            .Select(c => $"{c.CaseName}: {c.DeadlineText}")
            .ToList();
    }
}
