using JobPilot.Models;
using JobPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace JobPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<JobApplication> _store = new("applications.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var items = await _store.LoadAsync();
        TotalText.Text = items.Count.ToString();
        ActiveText.Text = items.Count(a => a.IsActive).ToString();
        InterviewText.Text = items.Count(a => a.Status == "Interview").ToString();
        OfferText.Text = items.Count(a => a.Status == "Offer").ToString();
        FollowUpList.ItemsSource = items
            .Where(a => a.FollowUpDate is not null && a.IsActive)
            .OrderBy(a => a.FollowUpDate)
            .Select(a => $"{a.Company} - {a.Role}: {a.FollowUpText}")
            .ToList();
    }
}
