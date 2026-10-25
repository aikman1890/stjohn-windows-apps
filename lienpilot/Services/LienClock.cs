using LienPilot.Models;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace LienPilot.Services;

/// <summary>
/// Computes one unit's lien timeline from its default date and the
/// operator-confirmed profile, using ACTUAL ledger dates for steps already
/// logged. Every milestone carries its basis text - no silent math.
/// </summary>
public static class LienClock
{
    public sealed class Milestone
    {
        public string Label { get; set; } = string.Empty;
        public DateTimeOffset? Date { get; set; }
        public string Basis { get; set; } = string.Empty;
        public bool Done { get; set; }
    }

    public sealed class Timeline
    {
        public List<Milestone> Milestones { get; set; } = new();
        public DateTimeOffset? EarliestSaleDate { get; set; }
        public string SaleBasis { get; set; } = string.Empty;
        public List<string> MissingSteps { get; set; } = new();
        public bool SaleReady { get; set; }
        public List<string> BlockReasons { get; set; } = new();
    }

    public static Brush BrushFor(DateTimeOffset? date, bool done)
    {
        if (done) return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
        if (date is not { } d) return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
        var days = (d.Date - DateTimeOffset.Now.Date).Days;
        if (days < 0) return new SolidColorBrush(Color.FromArgb(255, 200, 60, 60));
        if (days <= 5) return new SolidColorBrush(Color.FromArgb(255, 220, 140, 30));
        return new SolidColorBrush(Color.FromArgb(255, 60, 160, 80));
    }

    public static Timeline Compute(StorageUnit unit, StateProfile profile, List<LienLedgerEntry> ledger)
    {
        var t = new Timeline();
        var mine = ledger.Where(l => l.UnitId == unit.Id).ToList();
        LienLedgerEntry? Find(string step) => mine
            .Where(l => string.Equals(l.StepType, step, StringComparison.OrdinalIgnoreCase))
            .OrderBy(l => l.Date).FirstOrDefault();

        if (unit.DefaultDate is not { } def)
        {
            t.BlockReasons.Add("No default date recorded for this unit.");
            return t;
        }

        var noticeDue = def.AddDays(profile.NoticeDayOffset);
        var noticeEntry = Find("Lien notice");
        t.Milestones.Add(new Milestone
        {
            Label = "Lien notice",
            Date = noticeEntry?.Date ?? noticeDue,
            Done = noticeEntry is not null,
            Basis = noticeEntry is not null
                ? $"Sent {noticeEntry.Date:MMM d, yyyy} by {noticeEntry.Method} (per your ledger)"
                : $"Due {noticeDue:MMM d, yyyy} - {profile.NoticeDayOffset} days after default per your profile \"{profile.Name}\"",
        });

        DateTimeOffset? saleFromPub = null;
        if (profile.PublicationRequired)
        {
            var pub1 = Find("Advertisement week 1");
            var pub2 = Find("Advertisement week 2");
            t.Milestones.Add(new Milestone
            {
                Label = "Advertisement week 1",
                Date = pub1?.Date,
                Done = pub1 is not null,
                Basis = pub1 is not null ? $"Published {pub1.Date:MMM d, yyyy} (per your ledger)" : "Not yet recorded in the ledger",
            });
            t.Milestones.Add(new Milestone
            {
                Label = "Advertisement week 2",
                Date = pub2?.Date,
                Done = pub2 is not null,
                Basis = pub2 is not null ? $"Published {pub2.Date:MMM d, yyyy} (per your ledger)" : "Not yet recorded - publication runs two consecutive weeks per your profile",
            });
            if (pub1 is not null) saleFromPub = pub1.Date.AddDays(profile.SaleDaysAfterFirstPublication);
        }

        if (saleFromPub is { } sp)
        {
            t.EarliestSaleDate = sp;
            t.SaleBasis = $"Sale eligible {sp:MMM d, yyyy} - {profile.SaleDaysAfterFirstPublication} days after first publication per your profile \"{profile.Name}\"";
        }
        else if (noticeEntry is not null && !profile.PublicationRequired)
        {
            var d = noticeEntry.Date.AddDays(profile.WaitingDaysAfterNotice);
            t.EarliestSaleDate = d;
            t.SaleBasis = $"Sale eligible {d:MMM d, yyyy} - {profile.WaitingDaysAfterNotice} days after the lien notice per your profile \"{profile.Name}\"";
        }
        else if (!profile.PublicationRequired)
        {
            var d = noticeDue.AddDays(profile.WaitingDaysAfterNotice);
            t.EarliestSaleDate = d;
            t.SaleBasis = $"ESTIMATE: {d:MMM d, yyyy} if the notice goes out on its due date ({profile.WaitingDaysAfterNotice}-day wait per your profile). The real clock starts when the notice is actually sent and logged.";
        }
        else
        {
            t.SaleBasis = "Sale date cannot be computed until Advertisement week 1 is logged - your profile counts the sale window from first publication.";
        }

        foreach (var step in profile.RequiredSteps)
        {
            if (Find(step) is null) t.MissingSteps.Add(step);
        }

        if (!profile.Confirmed)
            t.BlockReasons.Add($"Profile \"{profile.Name}\" is NOT confirmed - verify it against your statute and counsel before relying on any date here.");
        if (t.MissingSteps.Count > 0)
            t.BlockReasons.Add($"Missing required step(s): {string.Join(", ", t.MissingSteps)}.");
        if (t.EarliestSaleDate is { } esd && esd.Date > DateTimeOffset.Now.Date)
            t.BlockReasons.Add($"Earliest sale date is {esd:MMM d, yyyy} - not yet reached.");
        if (t.EarliestSaleDate is null && profile.PublicationRequired)
            t.BlockReasons.Add("No sale date yet - see the basis note above.");
        if (unit.HasHold)
            t.BlockReasons.Add("A hold flag is set on this unit (bankruptcy / military / dispute) - do not sell while a hold flag stands.");
        if (unit.Balance <= 0)
            t.BlockReasons.Add("Balance is zero - nothing to lien.");

        t.SaleReady = t.BlockReasons.Count == 0;
        return t;
    }
}
