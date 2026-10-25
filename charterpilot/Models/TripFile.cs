namespace CharterPilot.Models;

public sealed class TripFile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string QuoteId { get; set; } = string.Empty;
    public string ClientLabel { get; set; } = string.Empty;
    public string RouteLabel { get; set; } = string.Empty;
    public string OperatorLabel { get; set; } = string.Empty;
    public string TailLabel { get; set; } = string.Empty;
    public DateTimeOffset DepartAt { get; set; } = DateTimeOffset.Now.AddDays(2);
    public bool AgreementSigned { get; set; }
    public bool OperatorConfirmed { get; set; }
    public bool PassengerListReceived { get; set; }
    public bool CateringOrdered { get; set; }
    public bool GroundTransportBooked { get; set; }
    public bool PaymentReceived { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal FinalOperatorCost { get; set; }
    public decimal OtherCosts { get; set; }
    public string Currency { get; set; } = "USD";
    public string Notes { get; set; } = string.Empty;

    public decimal MarginKept => FinalPrice - FinalOperatorCost - OtherCosts;

    public List<string> MissingItems
    {
        get
        {
            var missing = new List<string>();
            if (!AgreementSigned) missing.Add("signed charter agreement");
            if (!OperatorConfirmed) missing.Add("operator confirmation");
            if (!PassengerListReceived) missing.Add("passenger list");
            if (!CateringOrdered) missing.Add("catering order");
            if (!GroundTransportBooked) missing.Add("ground transport");
            if (!PaymentReceived) missing.Add("payment received");
            return missing;
        }
    }

    public bool IsReady => MissingItems.Count == 0;

    public string DisplayLine =>
        $"{DepartAt:MMM d, HH:mm} - {ClientLabel} - {RouteLabel} - {(IsReady ? "READY" : $"NOT READY: missing {string.Join(", ", MissingItems)}")} - margin kept {MarginKept:0.00} {Currency}";
}
