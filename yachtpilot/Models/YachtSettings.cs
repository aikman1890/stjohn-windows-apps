namespace YachtPilot.Models;

public sealed class YachtSettings
{
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "USD";
    public List<string> CertTypes { get; set; } = new()
    {
        "STCW Basic Safety", "ENG1 Medical", "Master Licence", "Yachtmaster",
        "Passport", "Visa / B1-B2", "Radio Licence", "Food Hygiene", "Other",
    };
}
