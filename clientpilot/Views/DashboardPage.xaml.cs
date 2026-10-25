using ClientPilot.Models;
using ClientPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ClientPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<ClientProspect> _store = new("prospects.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var items = await _store.LoadAsync();
        TotalText.Text = items.Count.ToString();
        ActiveText.Text = items.Count(p => p.IsActive).ToString();
        RepliesText.Text = items.Count(p => p.Status == "Replied").ToString();
        WonText.Text = items.Count(p => p.Status == "Won").ToString();
        FollowUpList.ItemsSource = items
            .Where(p => p.FollowUpDate is not null && p.IsActive)
            .OrderBy(p => p.FollowUpDate)
            .Select(p => $"{p.Company}: {p.FollowUpText}")
            .ToList();
    }
}
