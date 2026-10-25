namespace BayPilot.Models;

/// <summary>
/// Shop-level settings, entered by the shop and shown on every estimate
/// and invoice: identity for document headers, labor rate preset, tax
/// rate, and shop-supplies fee. Nothing here is hardcoded or fetched.
/// </summary>
public sealed class ShopSettings
{
    public string ShopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal LaborRate { get; set; } = 95m;
    public decimal TaxPercent { get; set; }
    public decimal SuppliesFee { get; set; }
    public decimal DefaultMarkupPercent { get; set; } = 50m;
}
