using WellPilot.Models;

namespace WellPilot.Services;

/// <summary>
/// The monthly close math. Decimal everywhere. Revenue credits = net revenue x deck
/// decimal for every interest class. JIB charges = the month's expenses allocated
/// across WORKING-interest owners pro-rata to their share of the total WI decimal.
/// Every figure is rounded half-even to cents; the rounding remainder lands on the
/// largest-interest owner of the class, and that rule is shown on every statement.
/// </summary>
public static class DistributionEngine
{
    private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.ToEven);

    public static List<StatementLine> Compute(
        DistributionRun run, List<DeckEntry> activeDeck, decimal netRevenue, decimal totalExpenses)
    {
        var lines = new List<StatementLine>();

        // Revenue credits.
        var credits = activeDeck.Select(d => (Entry: d, Raw: netRevenue * d.DecimalInterest)).ToList();
        var roundedCredits = credits.Select(c => Round2(c.Raw)).ToList();
        var creditRemainder = Round2(netRevenue * activeDeck.Sum(d => d.DecimalInterest)) - roundedCredits.Sum();
        if (credits.Count > 0 && creditRemainder != 0m)
        {
            var idx = credits.IndexOf(credits.OrderByDescending(c => c.Entry.DecimalInterest).First());
            roundedCredits[idx] += creditRemainder;
        }

        // JIB charges across WI owners only, pro-rata to WI share.
        var wiTotal = activeDeck.Where(d => d.InterestType == "Working interest").Sum(d => d.DecimalInterest);
        var charges = new Dictionary<string, decimal>();
        if (wiTotal > 0m && totalExpenses != 0m)
        {
            var wi = activeDeck.Where(d => d.InterestType == "Working interest").ToList();
            var raw = wi.Select(d => (Entry: d, Raw: totalExpenses * d.DecimalInterest / wiTotal)).ToList();
            var rounded = raw.Select(c => Round2(c.Raw)).ToList();
            var remainder = Round2(totalExpenses) - rounded.Sum();
            if (raw.Count > 0 && remainder != 0m)
            {
                var idx = raw.IndexOf(raw.OrderByDescending(c => c.Entry.DecimalInterest).First());
                rounded[idx] += remainder;
            }
            for (var i = 0; i < wi.Count; i++) charges[wi[i].Id] = rounded[i];
        }

        for (var i = 0; i < activeDeck.Count; i++)
        {
            var d = activeDeck[i];
            lines.Add(new StatementLine
            {
                RunId = run.Id,
                OwnerId = d.OwnerId,
                OwnerLabel = d.OwnerLabel,
                InterestType = d.InterestType,
                DecimalUsed = d.DecimalInterest,
                RevenueCredit = roundedCredits[i],
                JibCharge = charges.TryGetValue(d.Id, out var ch) ? ch : 0m,
            });
        }
        return lines;
    }
}
