using GalleryPilot.Models;
using GalleryPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GalleryPilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await new GallerySettingsStore().LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.GalleryName))
            TitleText.Text = $"{settings.GalleryName} - dashboard";

        var consignments = await new JsonListStore<Consignment>("consignments.json").LoadAsync();
        var sales = await new JsonListStore<Sale>("sales.json").LoadAsync();
        var loans = await new JsonListStore<Loan>("loans.json").LoadAsync();
        var works = await new JsonListStore<Artwork>("artworks.json").LoadAsync();
        var records = await new JsonListStore<RecordEntry>("records.json").LoadAsync();

        var active = consignments.Where(c => c.Status == "Active").OrderBy(c => c.TermEnd).ToList();
        ConsignText.Text = active.Count == 0 ? "No active consignments."
            : string.Join("\n", active.Take(6).Select(c => c.DisplayLine));

        var lines = new List<string>();
        foreach (var s in sales.Where(s => !s.IsPaid).OrderBy(s => s.DueDate))
            lines.Add($"OWED TO ARTIST: {s.ArtistLabel} - {s.ArtistOwed:0.00} {s.Currency} for {s.ArtworkLabel}" +
                (s.DaysOverdue > 0 ? $" - {s.DaysOverdue} DAYS PAST DUE" : $" - due {s.DueDate:MMM d}"));
        foreach (var l in loans.Where(l => !l.IsClosed).OrderBy(l => l.ReturnDue))
            lines.Add($"LOAN OUT: {l.ArtworkLabel} -> {l.Borrower} - {(l.DaysUntilReturn < 0 ? $"OVERDUE by {-l.DaysUntilReturn} days" : $"due back in {l.DaysUntilReturn} days")}");
        MoneyText.Text = lines.Count == 0 ? "No outstanding payables, no open loans." : string.Join("\n", lines);

        var gaps = works.Where(wk => wk.Status != "Sold" && wk.Status != "Returned"
                && !records.Any(r => r.ArtworkId == wk.Id && r.Kind == "Condition"))
            .Select(wk => $"No condition entry on file: {wk.Artist} - \"{wk.Title}\" (currently {wk.Location}). A condition report not written becomes an argument later.")
            .ToList();
        GapsText.Text = gaps.Count == 0 ? "Every held work has a condition entry." : string.Join("\n", gaps);
    }
}
