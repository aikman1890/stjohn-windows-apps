using AgreementPilot.Models;
using AgreementPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AgreementPilot.Views;

public sealed partial class TuneUpsPage : Page
{
    private readonly JsonListStore<Agreement> _store = new("agreements.json");

    public TuneUpsPage()
    {
        this.InitializeComponent();
    }

    private static string Line(Agreement a) =>
        $"{a.CustomerLabel} - {a.PlanName} - {a.VisitsOwed} visit(s) owed ({a.VisitsDelivered}/{a.VisitsIncluded} delivered) - renews {a.RenewalDate:MMM d, yyyy}";

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var agreements = (await _store.LoadAsync())
            .Where(a => a.Status is "Active" or "Renewal due" && a.VisitsOwed > 0)
            .Where(a => a.VisitsOwed > 0)
            .OrderBy(a => a.RenewalDate).ToList();

        SpringList.ItemsSource = agreements.Where(a => a.RenewalDate.Month is >= 3 and <= 5).Select(Line).ToList();
        FallList.ItemsSource = agreements.Where(a => a.RenewalDate.Month is >= 9 and <= 11).Select(Line).ToList();
        OtherList.ItemsSource = agreements.Where(a => a.RenewalDate.Month is < 3 or (>= 6 and <= 8) or > 11).Select(Line).ToList();
    }
}
