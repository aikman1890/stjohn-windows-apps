namespace BayPilot.Models;

/// <summary>
/// One estimate/invoice line on a repair order. Labor lines price as
/// hours x rate. Part lines carry cost and markup; the selling price is
/// computed from them. Every line carries the customer's decision -
/// approved, declined (which feeds the declined-work register), or
/// pending - because the approval trail is the product.
/// </summary>
public sealed class RoLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Kind { get; set; } = "Labor";
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public decimal Rate { get; set; }
    public decimal PartCost { get; set; }
    public decimal MarkupPercent { get; set; }
    public string Decision { get; set; } = "Pending";
    public string DeclinedReason { get; set; } = string.Empty;
    public DateTimeOffset? FollowUpDate { get; set; }
    public bool FollowUpDone { get; set; }

    public decimal Amount => Quantity * Rate;

    public string DisplayLine =>
        $"{Kind}: {Description} - {Quantity:0.##} x {Rate:C} = {Amount:C} [{Decision}]";
}

/// <summary>
/// One repair order: the document at the center of the shop's day. It
/// carries the estimate lines, the status pipeline, promised time, tech,
/// mileage in/out, and the recommendations that carry forward to the
/// vehicle's next visit.
/// </summary>
public sealed class RepairOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RoNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerLabel { get; set; } = string.Empty;
    public Guid VehicleId { get; set; }
    public string VehicleLabel { get; set; } = string.Empty;
    public string Status { get; set; } = "Checked in";
    public DateTimeOffset OpenedDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? PromisedDate { get; set; }
    public string Tech { get; set; } = string.Empty;
    public int MileageIn { get; set; }
    public int MileageOut { get; set; }
    public string Complaint { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public List<RoLine> Lines { get; set; } = new();

    public bool IsOpen => Status is not ("Done" or "Picked up");

    public decimal Subtotal => Lines.Where(l => l.Decision == "Approved").Sum(l => l.Amount);

    public decimal AllLinesTotal => Lines.Sum(l => l.Amount);

    public string DisplayLine => $"RO {RoNumber} - {VehicleLabel}";

    public string SubLine =>
        $"{Status} - opened {OpenedDate:MMM d}" +
        (PromisedDate is { } p ? $" - promised {p:MMM d, h:mm tt}" : "") +
        (string.IsNullOrWhiteSpace(Tech) ? "" : $" - {Tech}");
}
