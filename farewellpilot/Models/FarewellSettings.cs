namespace FarewellPilot.Models;

public sealed class FarewellSettings
{
    public string HomeName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    /// <summary>Death-certificate filing window in days, from the home's own state rule. User-confirmed.</summary>
    public int DcFilingDays { get; set; } = 10;
    /// <summary>Preneed deposit window in days, from the home's own trust requirement. User-confirmed.</summary>
    public int PreneedDepositDays { get; set; } = 10;
    public List<string> BaseChecklist { get; set; } = new()
    {
        "Vital-records data gathered",
        "Obituary drafted",
        "Obituary placed",
        "Clergy / celebrant confirmed",
        "Cemetery or crematory scheduled",
        "Merchandise selected",
        "Flowers ordered",
        "Music selected",
        "Register book and stationery ready",
    };
    public List<string> BurialExtras { get; set; } = new()
    {
        "Burial permit confirmed",
        "Vault ordered",
        "Gravesite opening confirmed",
    };
    public List<string> CremationExtras { get; set; } = new()
    {
        "Cremation authorization signed",
        "Crematory scheduled",
        "Urn selected",
    };
}
