using DisputePilot.Models;
using DisputePilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DisputePilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<DisputeItem> _store = new("disputes.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var items = await _store.LoadAsync();
        TotalText.Text = items.Count.ToString();
        AwaitingText.Text = items.Count(d => d.Status == "Awaiting validation").ToString();
        OverdueText.Text = items.Count(d =>
            d.DaysRemaining < 0 && d.Status is not ("Deleted from reports" or "Settled" or "Closed")).ToString();
        DeletedText.Text = items.Count(d => d.Status is "Deleted from reports" or "Settled").ToString();
        DeadlineList.ItemsSource = items
            .OrderBy(d => d.Deadline)
            .Select(d => $"{d.Creditor}: {d.DeadlineText}")
            .ToList();
    }
}
