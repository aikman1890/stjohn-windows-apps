using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace RallyPilot.Models;

public sealed class EventPackage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Components { get; set; } = string.Empty;
    public decimal PerHeadPrice { get; set; }
    public decimal PerGroupPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int MinHeads { get; set; }
    public string CapacityNote { get; set; } = string.Empty;
    public string StaffingNote { get; set; } = string.Empty;

    public string DisplayLine => $"{Name} - {(PerHeadPrice > 0 ? $"{PerHeadPrice:0.00} {Currency}/head" : $"{PerGroupPrice:0.00} {Currency}/group")}" +
        (MinHeads > 0 ? $" - min {MinHeads}" : string.Empty) +
        (string.IsNullOrWhiteSpace(Components) ? string.Empty : $" - {Components}");
}

public sealed class AddOnDef
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PricingKind { get; set; } = "Per head"; // Per head | Per group | Flat
    public string Currency { get; set; } = "USD";

    public string DisplayLine => $"{Name} - {Price:0.00} {Currency} ({PricingKind.ToLowerInvariant()})";
}

public sealed class Quote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CompanyLabel { get; set; } = string.Empty;
    public string OrganizerLabel { get; set; } = string.Empty;
    public string PackageLabel { get; set; } = string.Empty;
    public int Heads { get; set; }
    public DateTimeOffset EventDate { get; set; } = DateTimeOffset.Now.AddDays(30);
    public string DepositTerms { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Draft | Sent | Won | Lost | Booked
    public DateTimeOffset? SentDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";

    public int? DaysWaiting => SentDate is { } s && Status == "Sent" ? (DateTimeOffset.Now.Date - s.Date).Days : null;

    public string StatusLine => Status == "Sent" && SentDate is { } s
        ? $"Sent {s:MMM d} - {(DaysWaiting == 0 ? "sent today" : $"unanswered {DaysWaiting} day(s) - follow up {(DaysWaiting >= 3 ? "OVERDUE" : "due")}")}"
        : Status;

    public SolidColorBrush FlagBrush => new(Status != "Sent" ? Colors.Gray
        : DaysWaiting >= 3 ? Colors.Firebrick
        : DaysWaiting >= 2 ? Colors.DarkOrange
        : Colors.SeaGreen);
}

public sealed class QuoteLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuoteId { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Qty { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Qty * UnitPrice;

    public string DisplayLine => $"{Label} - {Qty:0.##} x {UnitPrice:0.00} = {LineTotal:0.00}";
}

public sealed class RallyEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuoteId { get; set; }
    public string CompanyLabel { get; set; } = string.Empty;
    public string OrganizerLabel { get; set; } = string.Empty;
    public string OrganizerContact { get; set; } = string.Empty;
    public string QuoteRef { get; set; } = string.Empty;
    public DateTimeOffset EventDate { get; set; } = DateTimeOffset.Now.AddDays(14);
    public int ExpectedHeads { get; set; }
    public int ConfirmedHeads { get; set; }
    public decimal QuotedTotal { get; set; }
    public decimal DepositDue { get; set; }
    public decimal DepositPaid { get; set; }
    public decimal PaymentsReceived { get; set; }
    public DateTimeOffset? BalanceDueDate { get; set; }
    public string Currency { get; set; } = "USD";
    public string SpecialRequirements { get; set; } = string.Empty;
    public string Status { get; set; } = "Planning"; // Planning | Ready | Completed | Cancelled
    public bool ThankYouSent { get; set; }
    public DateTimeOffset? RebookPromptDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public int? BalanceDays => BalanceDueDate is { } d ? (d.Date - DateTimeOffset.Now.Date).Days : null;

    public string DisplayLine => $"{EventDate:MMM d, yyyy} - {CompanyLabel} ({OrganizerLabel}) - {ConfirmedHeads}/{ExpectedHeads} heads" +
        $" - quoted {QuotedTotal:0.00} {Currency} - {Status}" +
        (BalanceDueDate is { } d ? $" - balance due {d:MMM d} ({BalanceDays} days)" : string.Empty);

    public SolidColorBrush FlagBrush => new(BalanceDays switch
    {
        null => Colors.Gray,
        < 0 => Colors.Firebrick,
        <= 3 => Colors.DarkOrange,
        _ => Colors.SeaGreen,
    });
}

public sealed class RosterEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string EventLabel { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool WaiverSigned { get; set; }
    public string WaiverVersion { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public string DisplayLine => $"{Name} - {(WaiverSigned ? $"waiver SIGNED ({WaiverVersion})" : "WAIVER UNSIGNED - chase")}" +
        (string.IsNullOrWhiteSpace(Note) ? string.Empty : $" - {Note}");
}

public sealed class RunBlock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string StartNote { get; set; } = "09:00";
    public string EndNote { get; set; } = "10:00";
    public string Title { get; set; } = string.Empty;
    public string Kind { get; set; } = "Activity"; // Activity | Meal | Buffer | Briefing | Transport
    public string GroupLabel { get; set; } = string.Empty;
    public string StaffLabel { get; set; } = string.Empty;
    public string AssetStatusNote { get; set; } = string.Empty;

    public string DisplayLine => $"{StartNote}-{EndNote} - [{Kind}] {Title}" +
        (string.IsNullOrWhiteSpace(GroupLabel) ? string.Empty : $" - group {GroupLabel}") +
        (string.IsNullOrWhiteSpace(StaffLabel) ? string.Empty : $" - staff {StaffLabel}") +
        (string.IsNullOrWhiteSpace(AssetStatusNote) ? string.Empty : $" - asset: {AssetStatusNote}");
}

public sealed class AddOnSale
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string EventLabel { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Qty { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

    public decimal LineTotal => Qty * UnitPrice;
    public string DisplayLine => $"{Date:MMM d} - {Label} - {Qty:0.##} x {UnitPrice:0.00} = {LineTotal:0.00}";
}

public sealed class Organizer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Company { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string TypicalSize { get; set; } = string.Empty;
    public string BooksAnnually { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Company} - {ContactName} ({Contact})" +
        (string.IsNullOrWhiteSpace(TypicalSize) ? string.Empty : $" - typically {TypicalSize}") +
        (string.IsNullOrWhiteSpace(BooksAnnually) ? string.Empty : $" - {BooksAnnually}");
}

public sealed class RallySettings
{
    public string BusinessName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "USD";
    public bool SampleSeeded { get; set; }
}
