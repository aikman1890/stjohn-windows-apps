using InvoicePilot.Models;
using InvoicePilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace InvoicePilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<Invoice> _store = new("invoices.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var items = await _store.LoadAsync();
        TotalText.Text = items.Count.ToString();
        OutstandingText.Text = items
            .Where(i => i.Status is "Issued" or "Overdue")
            .Sum(i => i.Total)
            .ToString("C");
        PaidText.Text = items
            .Where(i => i.Status == "Paid")
            .Sum(i => i.Total)
            .ToString("C");
        DraftText.Text = items.Count(i => i.Status == "Draft").ToString();
        RecentList.ItemsSource = items
            .OrderByDescending(i => i.IssueDate)
            .Take(10)
            .Select(i => $"{i.InvoiceNumber} - {i.ClientName}: {i.Total:C} ({i.Status})")
            .ToList();
    }
}
