using GalleryPilot.Models;
using Windows.Storage;

namespace GalleryPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("gallery-seeded.flag") is not null) return;

        var workStore = new JsonListStore<Artwork>("artworks.json");
        if ((await workStore.LoadAsync()).Count > 0)
        {
            await folder.CreateFileAsync("gallery-seeded.flag", CreationCollisionOption.ReplaceExisting);
            return;
        }

        // FICTIONAL artists, works, and collectors.
        var con1 = new Consignment
        {
            ConsignorName = "Sample Artist - Mara Ellison (fictional)", ConsignorKind = "Artist",
            Contact = "artist@sample.invalid", SplitPercent = 50m,
            StartDate = DateTimeOffset.Now.AddMonths(-5), TermEnd = DateTimeOffset.Now.AddDays(12),
            TermsNote = "SAMPLE: term ends in 12 days - inside the orange window; decide renewal or return.",
        };
        var con2 = new Consignment
        {
            ConsignorName = "Sample Estate of J. Okafor (fictional)", ConsignorKind = "Estate",
            Contact = "estate@sample.invalid", SplitPercent = 60m,
            StartDate = DateTimeOffset.Now.AddMonths(-2), TermEnd = DateTimeOffset.Now.AddMonths(4),
        };
        await new JsonListStore<Consignment>("consignments.json").SaveAsync(new List<Consignment> { con1, con2 });

        var w1 = new Artwork { Artist = "Mara Ellison (fictional)", Title = "Low Water, Sample Bay", Year = "2024", Medium = "Oil on linen (sample)", Dimensions = "48 x 60 in (sample)", Location = "Gallery", Status = "Available", AskingPrice = 12500m, Currency = "USD", ConsignmentId = con1.Id };
        var w2 = new Artwork { Artist = "Mara Ellison (fictional)", Title = "Field Study No. 3 (sample)", Year = "2023", Medium = "Oil on panel (sample)", Dimensions = "24 x 30 in (sample)", Location = "Gallery", Status = "Sold", AskingPrice = 6800m, Currency = "USD", ConsignmentId = con1.Id };
        var w3 = new Artwork { Artist = "J. Okafor (fictional)", Title = "Harbor Archive (sample)", Year = "1998", Medium = "Mixed media (sample)", Dimensions = "36 x 36 in (sample)", Location = "On loan", Status = "Available", AskingPrice = 21000m, Currency = "USD", ConsignmentId = con2.Id };
        var w4 = new Artwork { Artist = "Theo Lindqvist (fictional)", Title = "North Light (sample)", Year = "2025", Medium = "Photograph, ed. 2/6 (sample)", Dimensions = "30 x 40 in (sample)", Edition = "2/6", Location = "Storage", Status = "Available", AskingPrice = 4200m, Currency = "USD" };
        await workStore.SaveAsync(new List<Artwork> { w1, w2, w3, w4 });

        await new JsonListStore<RecordEntry>("records.json").SaveAsync(new List<RecordEntry>
        {
            new() { ArtworkId = w1.Id, ArtworkLabel = $"{w1.Artist} - {w1.Title}", Kind = "Provenance", Detail = "Acquired from the artist's studio on consignment (sample entry)." },
            new() { ArtworkId = w1.Id, ArtworkLabel = $"{w1.Artist} - {w1.Title}", Kind = "Condition", Moment = "Intake", Detail = "Excellent; no surface issues noted at intake (sample).", PhotoPath = "C:\\SamplePhotos\\intake-w1.jpg (sample path)" },
            new() { ArtworkId = w3.Id, ArtworkLabel = $"{w3.Artist} - {w3.Title}", Kind = "Condition", Moment = "Before loan", Detail = "Frame corner wear noted and photographed before loan (sample)." },
        });

        var collector = new CollectorContact
        {
            Name = "Sample Collector - R. Alvarez (fictional)", Contact = "collector@sample.invalid",
            Interests = "Coastal painting, mid-career (sample)", Notes = "Prefers private viewings on Saturdays (sample).",
        };
        await new JsonListStore<CollectorContact>("collectors.json").SaveAsync(new List<CollectorContact> { collector });

        // The unpaid payable: sold 20 days ago, due in 5, at the consignment's 50% split.
        await new JsonListStore<Sale>("sales.json").SaveAsync(new List<Sale>
        {
            new()
            {
                ArtworkId = w2.Id, ArtworkLabel = $"{w2.Artist} - \"{w2.Title}\"", ArtistLabel = con1.ConsignorName,
                CollectorLabel = collector.Name, Date = DateTimeOffset.Now.AddDays(-20),
                SalePrice = 6800m, Currency = "USD", SplitPercent = 50m,
                DueDate = DateTimeOffset.Now.AddDays(5), Note = "SAMPLE: artist payable outstanding - pay on time, with the statement.",
            },
        });

        await new JsonListStore<Loan>("loans.json").SaveAsync(new List<Loan>
        {
            new()
            {
                ArtworkId = w3.Id, ArtworkLabel = $"{w3.Artist} - \"{w3.Title}\"",
                Borrower = "Sample Regional Museum (fictional)", Venue = "Sample Museum, Hall 2",
                StartDate = DateTimeOffset.Now.AddMonths(-1), ReturnDue = DateTimeOffset.Now.AddDays(25),
                ConditionOutLogged = true, Notes = "SAMPLE loan - condition-in entry still owed at return.",
            },
        });

        await folder.CreateFileAsync("gallery-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
