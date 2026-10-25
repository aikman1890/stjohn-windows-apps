namespace GuardPilot.Models;

public sealed class ClientSite
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Client { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContractRef { get; set; } = string.Empty;
    public decimal BillRatePerHour { get; set; }
    public string BillingTerms { get; set; } = string.Empty;
    public string SiteContact { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => $"{Client} - {SiteName}";
}
