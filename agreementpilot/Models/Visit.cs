namespace AgreementPilot.Models;

public sealed class Visit
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerLabel { get; set; } = string.Empty;
    public string EquipmentId { get; set; } = string.Empty;
    public string EquipmentLabel { get; set; } = string.Empty;
    public string AgreementId { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public string WorkPerformed { get; set; } = string.Empty;
    public string Readings { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string RecResponse { get; set; } = "None"; // None / Approved / Declined
    public decimal RecAmount { get; set; }
    public bool FollowUpDone { get; set; }
    public DateTimeOffset? NextVisitDate { get; set; }

    public string DisplayLine =>
        $"{Date:MMM d, yyyy} - {EquipmentLabel} - {WorkPerformed}" +
        (string.IsNullOrWhiteSpace(Recommendation) ? string.Empty : $" - rec: {Recommendation} [{RecResponse}]");
}
