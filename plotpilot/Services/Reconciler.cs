using PlotPilot.Models;

namespace PlotPilot.Services;

/// <summary>
/// Compares the three views of the truth - the register, the burial records, and
/// the ownership chain - and lists where they disagree, for the board to work
/// through deliberately. PlotPilot flags conflicts; it never settles them.
/// </summary>
public static class Reconciler
{
    public static List<string> FindConflicts(
        List<PlotSpace> spaces, List<BurialRecord> burials, List<ChainEntry> chain)
    {
        var conflicts = new List<string>();
        var burialsBySpace = burials.GroupBy(b => b.SpaceId).ToDictionary(g => g.Key, g => g.Count());
        foreach (var space in spaces.OrderBy(s => s.Address))
        {
            var count = burialsBySpace.TryGetValue(space.Id, out var c) ? c : 0;
            if (space.Status == "Occupied" && count == 0)
                conflicts.Add($"{space.Address}: marked Occupied (plat/register) but NO burial record exists - check the notebooks before this space is relied on.");
            if (space.Status != "Occupied" && count > 0)
                conflicts.Add($"{space.Address}: has {count} burial record(s) but the space status is {space.Status}, not Occupied.");
            if ((space.Status is "Sold" or "Occupied") && string.IsNullOrWhiteSpace(space.OwnerName))
                conflicts.Add($"{space.Address}: status is {space.Status} but no owner of record is named.");
            var latest = chain.Where(x => x.SpaceId == space.Id && x.Kind is "Sale" or "Transfer" or "Inheritance")
                .OrderByDescending(x => x.Date).FirstOrDefault();
            if (latest is not null && !string.IsNullOrWhiteSpace(latest.ToName) &&
                !string.IsNullOrWhiteSpace(space.OwnerName) &&
                !string.Equals(latest.ToName.Trim(), space.OwnerName.Trim(), StringComparison.OrdinalIgnoreCase))
                conflicts.Add($"{space.Address}: register owner \"{space.OwnerName}\" differs from the latest chain entry \"{latest.ToName}\" ({latest.Date:MMM d, yyyy}) - the board decides which record governs.");
            if (space.Status == "Unknown")
                conflicts.Add($"{space.Address}: status Unknown - old records gap; kept honest rather than guessed.");
        }
        foreach (var burial in burials)
        {
            if (!spaces.Any(s => s.Id == burial.SpaceId))
                conflicts.Add($"Burial record for {burial.DecedentName} points at a space that is no longer in the register ({burial.SpaceLabel}).");
        }
        return conflicts;
    }
}
