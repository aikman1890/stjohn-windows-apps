using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace RangePilot.Models;

public sealed class ExperiencePackage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string DurationNote { get; set; } = string.Empty;
    public int Capacity { get; set; } = 6;
    public string AssetsRequired { get; set; } = string.Empty;
    public string IncludesNote { get; set; } = string.Empty;

    public string DisplayLine => $"{Name} - {Price:0.00} {Currency} - up to {Capacity} - {DurationNote}" +
        (string.IsNullOrWhiteSpace(AssetsRequired) ? string.Empty : $" - needs: {AssetsRequired}");
}

public sealed class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PackageLabel { get; set; } = string.Empty;
    public string LeadGuest { get; set; } = string.Empty;
    public int PartySize { get; set; } = 2;
    public DateTimeOffset SlotDate { get; set; } = DateTimeOffset.Now;
    public string SlotTimeNote { get; set; } = "10:00";
    public string Source { get; set; } = "Direct"; // Direct | Phone | Groupon | LivingSocial | Auction | Gift card
    public decimal TotalPrice { get; set; }
    public decimal DepositPaid { get; set; }
    public string Currency { get; set; } = "USD";
    public string AssetLabel { get; set; } = string.Empty;
    public string StaffNote { get; set; } = string.Empty;
    public string Status { get; set; } = "Booked"; // Booked | Completed | Cancelled | No-show
    public string Notes { get; set; } = string.Empty;

    public decimal BalanceDue => TotalPrice - DepositPaid;

    public string DisplayLine => $"{SlotDate:MMM d} {SlotTimeNote} - {PackageLabel} - {LeadGuest} x{PartySize}" +
        $" - total {TotalPrice:0.00} {Currency}, balance {BalanceDue:0.00} - via {Source} - {Status}" +
        (string.IsNullOrWhiteSpace(AssetLabel) ? string.Empty : $" - asset: {AssetLabel}");
}

public sealed class Participant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public string BookingLabel { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool WaiverSigned { get; set; }
    public bool IsMinor { get; set; }
    public string GuardianSigner { get; set; } = string.Empty;
    public string WaiverVersion { get; set; } = string.Empty;
    public string SignedNote { get; set; } = string.Empty;

    public string DisplayLine => $"{Name} - {(WaiverSigned ? $"waiver SIGNED ({WaiverVersion})" : "WAIVER UNSIGNED")}" +
        (IsMinor ? $" - MINOR, guardian: {GuardianSigner}" : string.Empty) +
        (string.IsNullOrWhiteSpace(SignedNote) ? string.Empty : $" - {SignedNote}");
}

public sealed class AssetItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "Vehicle"; // Vehicle | Range lane | Equipment | Other
    public string Status { get; set; } = "Up"; // Up | Down | Maintenance
    public string LastServiceNote { get; set; } = string.Empty;
    public string NextServiceNote { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Name} ({Kind}) - {Status.ToUpperInvariant()}" +
        (string.IsNullOrWhiteSpace(NextServiceNote) ? string.Empty : $" - next: {NextServiceNote}");

    public SolidColorBrush FlagBrush => new(Status switch
    {
        "Up" => Colors.SeaGreen,
        "Maintenance" => Colors.DarkOrange,
        _ => Colors.Firebrick,
    });
}

public sealed class AssetLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssetId { get; set; }
    public string AssetLabel { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string Kind { get; set; } = "Maintenance"; // Maintenance | Usage | Note
    public string Detail { get; set; } = string.Empty;

    public string DisplayLine => $"{Date:MMM d, yyyy} - {Kind} - {Detail}";
}

public sealed class StaffMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "Safety officer"; // Safety officer | Driver / instructor | Guide | Other
    public string Certifications { get; set; } = string.Empty;
    public DateTimeOffset? CertExpiry { get; set; }
    public string ScheduleNote { get; set; } = string.Empty;
    public bool Active { get; set; } = true;

    public int? CertDays => CertExpiry is { } x ? (x.Date - DateTimeOffset.Now.Date).Days : null;

    public string DisplayLine => $"{Name} - {Role} - certs: {Certifications}" +
        (CertExpiry is { } x ? $" - cert to {x:MMM d, yyyy} ({CertDays} days)" : " - no cert date recorded") +
        (Active ? string.Empty : " - INACTIVE");

    public SolidColorBrush FlagBrush => new(CertDays switch
    {
        null => Colors.Gray,
        < 0 => Colors.Firebrick,
        <= 30 => Colors.DarkOrange,
        _ => Colors.SeaGreen,
    });
}

public sealed class MediaItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BookingLabel { get; set; } = string.Empty;
    public string GuestLabel { get; set; } = string.Empty;
    public string Kind { get; set; } = "Video package"; // Photo package | Video package | Photo + video
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTimeOffset EventDate { get; set; } = DateTimeOffset.Now;
    public bool Captured { get; set; }
    public DateTimeOffset? DeliveredDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public bool Delivered => DeliveredDate is not null;
    public int DaysWaiting => (DateTimeOffset.Now.Date - EventDate.Date).Days;

    public string DisplayLine => Delivered
        ? $"DELIVERED {DeliveredDate:MMM d} - {Kind} for {GuestLabel} ({BookingLabel}) - {Price:0.00} {Currency}"
        : $"{(DaysWaiting > 2 ? "OVERDUE" : DaysWaiting == 2 ? "DUE NOW" : "OWED")} - {Kind} for {GuestLabel} ({BookingLabel}) - event {EventDate:MMM d} - captured: {(Captured ? "yes" : "NOT YET")} - {Price:0.00} {Currency}";

    public SolidColorBrush FlagBrush => new(Delivered ? Colors.SeaGreen
        : DaysWaiting > 2 ? Colors.Firebrick
        : DaysWaiting == 2 ? Colors.DarkOrange
        : Colors.Gray);
}

public sealed class RangeGuest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string OccasionNote { get; set; } = string.Empty;
    public int TotalVisits { get; set; }
    public DateTimeOffset? LastVisitDate { get; set; }
    public string ReferralNote { get; set; } = string.Empty;

    public int? DaysSinceVisit => LastVisitDate is { } x ? (DateTimeOffset.Now.Date - x.Date).Days : null;

    public string DisplayLine => $"{Name} - {TotalVisits} visit(s)" +
        (LastVisitDate is { } x ? $", last {x:MMM d, yyyy} ({DaysSinceVisit} days ago)" : ", no visit recorded") +
        (string.IsNullOrWhiteSpace(OccasionNote) ? string.Empty : $" - {OccasionNote}") +
        (string.IsNullOrWhiteSpace(ReferralNote) ? string.Empty : $" - {ReferralNote}");
}

public sealed class GiftCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string IssuedTo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Redeemed { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTimeOffset IssuedDate { get; set; } = DateTimeOffset.Now;
    public string Notes { get; set; } = string.Empty;

    public decimal Balance => Amount - Redeemed;
    public string DisplayLine => $"{Code} - to {IssuedTo} - {Amount:0.00} {Currency}, redeemed {Redeemed:0.00}, balance {Balance:0.00} - issued {IssuedDate:MMM d, yyyy}";
}

public sealed class RangeSettings
{
    public string BusinessName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "USD";
    public bool SampleSeeded { get; set; }
}
