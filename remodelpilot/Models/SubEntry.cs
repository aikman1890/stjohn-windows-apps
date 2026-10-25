namespace RemodelPilot.Models;

public sealed class SubEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string JobId { get; set; } = string.Empty;
    public string JobLabel { get; set; } = string.Empty;
    public string Who { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public decimal AgreedPrice { get; set; }
    public decimal PaidToDate { get; set; }
    public decimal Balance => AgreedPrice - PaidToDate;
    public bool WaiverReceived { get; set; }
    public DateTimeOffset? WaiverDate { get; set; }
    public bool CertReceived { get; set; }
    public DateTimeOffset? CertDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{Who} - {JobLabel} - agreed {AgreedPrice:C}, paid {PaidToDate:C}, balance {Balance:C}" +
        (WaiverReceived ? " - waiver on file" : " - NO waiver") +
        (CertReceived ? " - cert on file" : string.Empty);
}
