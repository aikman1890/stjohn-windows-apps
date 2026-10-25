using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace HuntPilot.Models;

public sealed class SpeciesEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal TrophyFee { get; set; }
    public string Currency { get; set; } = "USD";
    public string SeasonNote { get; set; } = string.Empty;
    public string MethodsNote { get; set; } = string.Empty;
    public string HerdNote { get; set; } = string.Empty;
    public string PackageNote { get; set; } = string.Empty;

    public string DisplayLine => $"{Name} - trophy fee {TrophyFee:0.00} {Currency}" +
        (string.IsNullOrWhiteSpace(SeasonNote) ? string.Empty : $" - {SeasonNote}") +
        (string.IsNullOrWhiteSpace(MethodsNote) ? string.Empty : $" - {MethodsNote}");
}

public sealed class Hunter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseState { get; set; } = string.Empty;
    public DateTimeOffset? LicenseExpiry { get; set; }
    public string PartyNote { get; set; } = string.Empty;
    public string Preferences { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public int? LicenseDays => LicenseExpiry is { } x ? (x.Date - DateTimeOffset.Now.Date).Days : null;

    public string DisplayLine => $"{Name} - {Contact}" +
        (string.IsNullOrWhiteSpace(LicenseNumber) ? " - no license recorded" : $" - license {LicenseState} {LicenseNumber}" +
            (LicenseExpiry is { } x ? $" to {x:MMM d, yyyy} ({LicenseDays} days)" : string.Empty)) +
        (string.IsNullOrWhiteSpace(Preferences) ? string.Empty : $" - {Preferences}");

    public SolidColorBrush FlagBrush => new(LicenseDays switch
    {
        null => Colors.Gray,
        < 0 => Colors.Firebrick,
        <= 30 => Colors.DarkOrange,
        _ => Colors.SeaGreen,
    });
}

public sealed class HuntBooking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HunterId { get; set; }
    public string HunterLabel { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.Now.AddDays(2);
    public string GuideLabel { get; set; } = string.Empty;
    public string SpeciesTargeted { get; set; } = string.Empty;
    public string PackageLabel { get; set; } = string.Empty;
    public decimal PackagePrice { get; set; }
    public decimal DepositReceived { get; set; }
    public decimal TrophyFees { get; set; }
    public decimal ExtraCharges { get; set; }
    public decimal PaymentsReceived { get; set; }
    public string Currency { get; set; } = "USD";
    public int LodgingNights { get; set; }
    public int NonHunterGuests { get; set; }
    public decimal NonHunterRatePerNight { get; set; }
    public string WaiverStatus { get; set; } = "Unsigned"; // Signed | Partial | Unsigned
    public string WaiverVersion { get; set; } = string.Empty;
    public string ArrivalNote { get; set; } = string.Empty;
    public string RoomNote { get; set; } = string.Empty;
    public string Status { get; set; } = "Booked"; // Booked | In camp | Completed | Cancelled
    public string Notes { get; set; } = string.Empty;

    public decimal LodgingCharges => LodgingNights * NonHunterGuests * NonHunterRatePerNight;
    public decimal TotalCharges => PackagePrice + TrophyFees + ExtraCharges + LodgingCharges;
    public decimal BalanceDue => TotalCharges - DepositReceived - PaymentsReceived;

    public string DisplayLine => $"{StartDate:MMM d}-{EndDate:MMM d} - {HunterLabel} - {SpeciesTargeted}" +
        $" - guide {GuideLabel} - total {TotalCharges:0.00} {Currency}, balance {BalanceDue:0.00} - waiver {WaiverStatus} - {Status}";
}

public sealed class Harvest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HuntId { get; set; }
    public string HunterLabel { get; set; } = string.Empty;
    public string SpeciesLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string GuideLabel { get; set; } = string.Empty;
    public string Measurements { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public bool FieldDressed { get; set; }
    public DateTimeOffset? FieldDressedDate { get; set; }
    public decimal TrophyFee { get; set; }
    public string Currency { get; set; } = "USD";
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {SpeciesLabel} - {HunterLabel} (guide {GuideLabel})" +
        $" - fee {TrophyFee:0.00} {Currency} - field dressed: {(FieldDressed ? $"yes {FieldDressedDate:MMM d}" : "NOT YET")}" +
        (string.IsNullOrWhiteSpace(Measurements) ? string.Empty : $" - {Measurements}");
}

public sealed class TrophyCase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HarvestId { get; set; }
    public string HunterLabel { get; set; } = string.Empty;
    public string SpeciesLabel { get; set; } = string.Empty;
    public string Stage { get; set; } = "Field dressed";
    public DateTimeOffset StageDate { get; set; } = DateTimeOffset.Now;
    public string ProcessorName { get; set; } = string.Empty;
    public string ProcessorTicket { get; set; } = string.Empty;
    public string TaxidermistName { get; set; } = string.Empty;
    public string WorkOrder { get; set; } = string.Empty;
    public string ShipNote { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public static readonly string[] Stages =
    {
        "Field dressed", "At processor", "Meat processed", "Meat shipped/delivered",
        "At taxidermist", "Mount in progress", "Mount complete", "Crated", "Shipped", "Delivered",
    };

    public int StageIndex => Math.Max(0, Array.IndexOf(Stages, Stage));
    public bool Closed => Stage == "Delivered";
    public int DaysInStage => (DateTimeOffset.Now.Date - StageDate.Date).Days;

    public string DisplayLine => $"{SpeciesLabel} - {HunterLabel} - {Stage} ({DaysInStage} days in stage, since {StageDate:MMM d})" +
        (string.IsNullOrWhiteSpace(ProcessorTicket) ? string.Empty : $" - processor ticket {ProcessorTicket}") +
        (string.IsNullOrWhiteSpace(WorkOrder) ? string.Empty : $" - work order {WorkOrder}") +
        (string.IsNullOrWhiteSpace(ShipNote) ? string.Empty : $" - {ShipNote}");

    /// <summary>Set from settings when the chain board loads; colors the stage clock.</summary>
    public int NormDays { get; set; } = 30;

    public SolidColorBrush FlagBrush => new(Closed ? Colors.SeaGreen
        : DaysInStage > NormDays * 2 ? Colors.Firebrick
        : DaysInStage > NormDays ? Colors.DarkOrange
        : Colors.SeaGreen);
}

public sealed class HuntSettings
{
    public string RanchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "USD";
    public int StageNormDays { get; set; } = 30;
    public bool SampleSeeded { get; set; }
}
