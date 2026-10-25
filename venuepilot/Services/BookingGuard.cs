using VenuePilot.Models;

namespace VenuePilot.Services;

public static class BookingGuard
{
    /// <summary>
    /// The double-booking rule: a CONTRACTED event owns its space for its date.
    /// Returns the conflicting contracted event, or null. Holds never hard-block;
    /// they warn (see Warnings).
    /// </summary>
    public static VenueEvent? ContractConflict(VenueEvent candidate, List<VenueEvent> all)
    {
        if (candidate.Status != "Contracted") return null;
        return all.FirstOrDefault(o =>
            o.Id != candidate.Id && o.Status == "Contracted" &&
            o.Space == candidate.Space && o.EventDate.Date == candidate.EventDate.Date);
    }

    public static List<string> Warnings(VenueEvent candidate, List<VenueEvent> all)
    {
        var warnings = new List<string>();
        foreach (var o in all)
        {
            if (o.Id == candidate.Id || o.Space != candidate.Space ||
                o.EventDate.Date != candidate.EventDate.Date) continue;
            if (o.Status == "Hold")
                warnings.Add($"Heads-up: {o.ClientNames} also holds {o.Space} on {o.EventDate:MMM d, yyyy} (hold expires {o.HoldExpiry:MMM d}).");
            if (o.Status == "Contracted" && candidate.Status != "Contracted")
                warnings.Add($"Note: {o.Space} is already CONTRACTED on {o.EventDate:MMM d, yyyy} to {o.ClientNames}.");
        }
        return warnings;
    }
}
