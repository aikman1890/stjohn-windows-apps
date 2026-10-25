using System.Globalization;
using DenialPilot.Models;
using DenialPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DenialPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<DenialCase> _store = new("cases.json");

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await SampleSeed.EnsureAsync(_store);
        var items = await _store.LoadAsync();
        OpenText.Text = items.Count(c => !c.IsResolved).ToString();
        DollarsText.Text = items
            .Where(c => !c.IsResolved)
            .Sum(c => ParseAmount(c.AmountDenied))
            .ToString("C");
        WonText.Text = items.Count(c => c.Status is "Resolved - won" or "Resolved - paid").ToString();
        LostText.Text = items.Count(c => c.Status == "Resolved - lost").ToString();
        PressureList.ItemsSource = items
            .Where(c => !c.IsResolved)
            .OrderBy(c => c.AppealDaysRemaining)
            .Select(c => $"{c.Insurer} claim {c.ClaimNumber}: {c.AppealText}")
            .ToList();
    }

    private static decimal ParseAmount(string text)
    {
        var cleaned = text.Trim().TrimStart('$').Replace(",", "");
        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0m;
    }
}
