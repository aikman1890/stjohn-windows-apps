using BillPilot.Models;
using BillPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BillPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<BillItem> _store = new("bills.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var items = await _store.LoadAsync();
        TotalText.Text = items.Count.ToString();
        DisputedText.Text = items.Count(b => b.Status.StartsWith("Disputed")).ToString();
        PastDueText.Text = items.Count(b => !b.IsClosed && b.DaysRemaining < 0).ToString();
        PaidText.Text = items.Count(b => b.IsClosed).ToString();
        DueList.ItemsSource = items
            .Where(b => !b.IsClosed)
            .OrderBy(b => b.DueDate)
            .Select(b => $"{b.Provider}: {b.DueText}")
            .ToList();
    }
}
