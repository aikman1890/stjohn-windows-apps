using TruckPilot.Models;

namespace TruckPilot.Services;

/// <summary>
/// The standard IFTA worksheet arithmetic, run on the operator's own trip
/// and fuel records. Rates come ONLY from the operator-confirmed rate table;
/// rows without a confirmed rate are reported as missing, never estimated.
/// Averages use the standard 2-decimal MPG convention; taxable gallons use
/// the standard whole-gallon rounding. Money is decimal throughout.
/// </summary>
public static class WorksheetCalculator
{
    public sealed class Row
    {
        public string Jurisdiction { get; set; } = string.Empty;
        public decimal Miles { get; set; }
        public decimal TaxableGallons { get; set; }
        public decimal TaxPaidGallons { get; set; }
        public decimal NetTaxableGallons { get; set; }
        public decimal? Rate { get; set; }
        public decimal? TaxDue { get; set; }
    }

    public sealed class Result
    {
        public decimal TotalMiles { get; set; }
        public decimal TotalGallons { get; set; }
        public decimal AverageMpg { get; set; }
        public List<Row> Rows { get; set; } = new();
        public List<string> MissingRates { get; set; } = new();
        public decimal TotalTaxDue { get; set; }
        public bool Complete => MissingRates.Count == 0;
    }

    public static (int Year, int Quarter) QuarterOf(DateTimeOffset date) =>
        (date.Year, (date.Month - 1) / 3 + 1);

    public static bool InQuarter(DateTimeOffset date, int year, int quarter) =>
        date.Year == year && (date.Month - 1) / 3 + 1 == quarter;

    public static Result Compute(int year, int quarter, List<Trip> trips, List<FuelPurchase> fuels, List<RateTable> rates)
    {
        var qTrips = trips.Where(t => InQuarter(t.Date, year, quarter)).ToList();
        var qFuels = fuels.Where(f => InQuarter(f.Date, year, quarter)).ToList();

        var result = new Result
        {
            TotalMiles = qTrips.Sum(t => t.Miles),
            TotalGallons = qFuels.Sum(f => f.Gallons),
        };
        result.AverageMpg = result.TotalGallons > 0
            ? Math.Round(result.TotalMiles / result.TotalGallons, 2)
            : 0m;

        var jurisdictions = qTrips.Select(t => t.Jurisdiction.Trim())
            .Concat(qFuels.Select(f => f.Jurisdiction.Trim()))
            .Where(j => j.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(j => j)
            .ToList();

        foreach (var j in jurisdictions)
        {
            var miles = qTrips.Where(t => string.Equals(t.Jurisdiction.Trim(), j, StringComparison.OrdinalIgnoreCase)).Sum(t => t.Miles);
            var taxable = result.AverageMpg > 0
                ? Math.Round(miles / result.AverageMpg, 0, MidpointRounding.AwayFromZero)
                : 0m;
            var paid = qFuels.Where(f => string.Equals(f.Jurisdiction.Trim(), j, StringComparison.OrdinalIgnoreCase)).Sum(f => f.Gallons);
            var rateRow = rates.FirstOrDefault(r => r.Year == year && r.Quarter == quarter && r.Confirmed &&
                string.Equals(r.Jurisdiction.Trim(), j, StringComparison.OrdinalIgnoreCase));
            var row = new Row
            {
                Jurisdiction = j,
                Miles = miles,
                TaxableGallons = taxable,
                TaxPaidGallons = paid,
                NetTaxableGallons = taxable - paid,
                Rate = rateRow?.RatePerGallon,
            };
            if (rateRow is null)
            {
                result.MissingRates.Add(j);
            }
            else
            {
                row.TaxDue = Math.Round(row.NetTaxableGallons * rateRow.RatePerGallon, 2);
                result.TotalTaxDue += row.TaxDue.Value;
            }
            result.Rows.Add(row);
        }
        return result;
    }
}
