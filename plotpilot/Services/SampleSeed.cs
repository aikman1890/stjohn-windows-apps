using PlotPilot.Models;
using Windows.Storage;

namespace PlotPilot.Services;

public static class SampleSeed
{
    public static async Task SeedIfFirstRunAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        if (await folder.TryGetItemAsync("plot-seeded.flag") is not null) return;

        var store = new JsonListStore<PlotSpace>("spaces.json");
        if ((await store.LoadAsync()).Count > 0)
        {
            await folder.CreateFileAsync("plot-seeded.flag", CreationCollisionOption.ReplaceExisting);
            return;
        }

        // Fictional sample: "Brookmont Cemetery" - 2 sections x 30 spaces.
        var spaces = new List<PlotSpace>();
        var rng = new Random(42);
        string[] sections = { "Section A", "Section B" };
        var index = 0;
        foreach (var section in sections)
        {
            for (var lot = 1; lot <= 3; lot++)
            {
                for (var sp = 1; sp <= 10; sp++)
                {
                    index++;
                    var status = (index % 10) switch
                    {
                        1 or 2 or 3 => "Occupied",
                        4 or 5 => "Sold",
                        6 => "Reserved",
                        7 => "Unknown",
                        _ => "Available",
                    };
                    spaces.Add(new PlotSpace
                    {
                        Section = section,
                        Block = "Block 1",
                        Lot = $"Lot {lot}",
                        Space = sp.ToString(),
                        Status = status,
                        OwnerName = status is "Sold" or "Occupied" ? $"Sample Family {index}" : string.Empty,
                        DeedRef = status is "Sold" or "Occupied" ? $"Deed book 4, p. {100 + index} (sample)" : string.Empty,
                        PurchaseDate = status is "Sold" or "Occupied" ? DateTimeOffset.Now.AddYears(-(1 + rng.Next(30))) : null,
                        PurchasePrice = status is "Sold" or "Occupied" ? 400m + rng.Next(8) * 50m : 0m,
                        CareContribution = status is "Sold" or "Occupied" ? 100m : 0m,
                        Source = index % 2 == 0 ? "Notebook 1, p. 12 (sample)" : "Plat map annotation (sample)",
                    });
                }
            }
        }
        // The deliberate conflict: an Occupied space with no burial record (A/Lot 2/Space 5 = index 15).
        var conflictSpace = spaces[14];
        conflictSpace.Status = "Occupied";
        conflictSpace.OwnerName = "Sample Family 15";
        await store.SaveAsync(spaces);

        var occupied = spaces.Where(s => s.Status == "Occupied" && s.Id != conflictSpace.Id).Take(14).ToList();
        var burials = new List<BurialRecord>();
        var n = 0;
        foreach (var space in occupied)
        {
            n++;
            burials.Add(new BurialRecord
            {
                SpaceId = space.Id,
                SpaceLabel = space.Address,
                DecedentName = $"Sample Decedent {n}",
                BirthDate = DateTimeOffset.Now.AddYears(-(60 + n)).AddDays(-40),
                DeathDate = DateTimeOffset.Now.AddYears(-(1 + n % 12)).AddDays(-15),
                IntermentDate = DateTimeOffset.Now.AddYears(-(1 + n % 12)).AddDays(-10),
                FuneralHome = "Sample Funeral Home",
                MarkerNotes = n % 3 == 0 ? "Upright granite, double (sample)" : "Flat marker (sample)",
                Veteran = n % 5 == 0,
                Source = "Notebook 2, p. 30 (sample)",
            });
        }
        await new JsonListStore<BurialRecord>("burials.json").SaveAsync(burials);

        var chain = new List<ChainEntry>();
        foreach (var space in spaces.Where(s => s.Status is "Sold" or "Occupied").Take(6))
        {
            chain.Add(new ChainEntry
            {
                SpaceId = space.Id, SpaceLabel = space.Address,
                Date = space.PurchaseDate ?? DateTimeOffset.Now.AddYears(-5),
                Kind = "Sale", FromName = "Brookmont Cemetery (sample)", ToName = space.OwnerName,
                DeedRef = space.DeedRef, Amount = space.PurchasePrice, CareContribution = space.CareContribution,
                Source = "Deed book (sample)",
            });
        }
        await new JsonListStore<ChainEntry>("chain.json").SaveAsync(chain);

        await new JsonListStore<WorkLogEntry>("worklog.json").SaveAsync(new List<WorkLogEntry>
        {
            new()
            {
                Date = DateTimeOffset.Now.AddDays(-3), SpaceLabel = occupied[0].Address,
                Kind = "Marker setting", Note = "Sample entry - marker set and photographed for the file.",
                ByWhom = "Sample Sexton",
            },
        });
        await folder.CreateFileAsync("plot-seeded.flag", CreationCollisionOption.ReplaceExisting);
    }
}
