using System.Text.Json;
using TruckPilot.Models;
using TruckPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;

namespace TruckPilot.Views;

public sealed partial class DashboardPage : Page
{
    private readonly JsonListStore<Trip> _tripStore = new("trips.json");
    private readonly JsonListStore<FuelPurchase> _fuelStore = new("fuel.json");
    private readonly JsonListStore<RateTable> _rateStore = new("rates.json");
    private readonly ProfileStore _profileStore = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    private static async Task<decimal> LoadCurrentMpgAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync("current-mpg.json");
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("mpg", out var el)) return el.GetDecimal();
            }
        }
        catch { /* fall through to default */ }
        return 0m;
    }

    private async void MpgSaveButton_Click(object sender, RoutedEventArgs e)
    {
        var mpg = decimal.TryParse(MpgBox.Text.Trim(), out var v) ? v : 0m;
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            "current-mpg.json", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(new Dictionary<string, decimal> { ["mpg"] = mpg }));
        await LoadDataAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var profile = await _profileStore.LoadAsync();
        ProfileText.Text = string.IsNullOrWhiteSpace(profile.LegalName)
            ? "No operator profile yet - fill it in on the Settings page so exports are complete."
            : $"{profile.LegalName}{(string.IsNullOrWhiteSpace(profile.Dba) ? "" : $" dba {profile.Dba}")} - IFTA account {profile.IftaAccount} - base {profile.BaseJurisdiction} - unit {profile.UnitId}";

        var trips = await _tripStore.LoadAsync();
        var fuels = await _fuelStore.LoadAsync();
        var rates = await _rateStore.LoadAsync();
        var now = DateTimeOffset.Now;
        var (year, quarter) = WorksheetCalculator.QuarterOf(now);

        var qTrips = trips.Where(t => WorksheetCalculator.InQuarter(t.Date, year, quarter)).ToList();
        var qFuels = fuels.Where(f => WorksheetCalculator.InQuarter(f.Date, year, quarter)).ToList();
        var qMiles = qTrips.Sum(t => t.Miles);
        var qGallons = qFuels.Sum(f => f.Gallons);
        QuarterMilesText.Text = $"{year} Q{quarter}: {qMiles:N1} miles";
        QuarterFuelText.Text = $"{qGallons:N3} gallons purchased across {qFuels.Count} stop(s), {qTrips.Count} trip record(s)";

        var mpg = await LoadCurrentMpgAsync();
        if (mpg <= 0 && qGallons > 0) mpg = Math.Round(qMiles / qGallons, 2);
        MpgBox.Text = mpg > 0 ? mpg.ToString("0.00") : string.Empty;
        if (mpg > 0)
        {
            var taxable = Math.Round(qMiles / mpg, 0, MidpointRounding.AwayFromZero);
            QuarterEstimateText.Text =
                $"Running estimate at {mpg:0.00} MPG: about {taxable:N0} taxable gallons this quarter. " +
                "The Worksheet page computes the exact per-jurisdiction figures from your confirmed rates.";
        }
        else
        {
            QuarterEstimateText.Text = "Enter your current MPG to see a running taxable-gallon estimate.";
        }

        var ytdMiles = trips.Where(t => t.Date.Year == year).Sum(t => t.Miles);
        var ytdGallons = fuels.Where(f => f.Date.Year == year).Sum(f => f.Gallons);
        YearText.Text = $"{year}: {ytdMiles:N1} miles, {ytdGallons:N3} gallons, {trips.Count(t => t.Date.Year == year)} trip record(s).";

        var issues = new List<string>();
        var ordered = trips.OrderBy(t => t.Date).ThenBy(t => t.OdometerStart).ToList();
        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].OdometerStart < ordered[i - 1].OdometerEnd)
                issues.Add($"Odometer overlap: {ordered[i].Date:MMM d} starts at {ordered[i].OdometerStart:N0}, below the previous trip's end {ordered[i - 1].OdometerEnd:N0}.");
            else if (ordered[i].OdometerStart > ordered[i - 1].OdometerEnd)
                issues.Add($"Odometer gap: {ordered[i - 1].OdometerEnd:N0} to {ordered[i].OdometerStart:N0} before {ordered[i].Date:MMM d} is unexplained - deadhead or personal miles still belong in the record.");
        }
        foreach (var f in qFuels.Where(f => string.IsNullOrWhiteSpace(f.ReceiptRef)))
            issues.Add($"Fuel purchase {f.Date:MMM d} in {f.Jurisdiction} ({f.Gallons:N3} gal) has no receipt reference - auditors ask for receipts.");
        var jurisdictions = qTrips.Select(t => t.Jurisdiction.Trim()).Where(j => j.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var j in jurisdictions)
        {
            if (!rates.Any(r => r.Year == year && r.Quarter == quarter && r.Confirmed &&
                string.Equals(r.Jurisdiction.Trim(), j, StringComparison.OrdinalIgnoreCase)))
                issues.Add($"No confirmed {year} Q{quarter} rate for {j} - the worksheet cannot price that row yet.");
        }
        if (issues.Count == 0)
            issues.Add("No gaps found in the current quarter's records. Keep logging every trip and every gallon.");
        IssuesList.ItemsSource = issues;
    }
}
