using Windows.Storage;

namespace BizPilot.Models;

public sealed class BusinessProfile
{
    private const string KeyBusinessName = "BusinessProfile.BusinessName";
    private const string KeyAddress = "BusinessProfile.Address";
    private const string KeyEmail = "BusinessProfile.Email";
    private const string KeyPhone = "BusinessProfile.Phone";
    private const string KeyTaxRate = "BusinessProfile.TaxRate";

    public string BusinessName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public double TaxRate { get; set; } = 0.0825;

    public static BusinessProfile Load()
    {
        var values = ApplicationData.Current.LocalSettings.Values;
        var profile = new BusinessProfile
        {
            BusinessName = values[KeyBusinessName] as string ?? string.Empty,
            Address = values[KeyAddress] as string ?? string.Empty,
            Email = values[KeyEmail] as string ?? string.Empty,
            Phone = values[KeyPhone] as string ?? string.Empty
        };

        if (values[KeyTaxRate] is double rate)
        {
            profile.TaxRate = rate;
        }

        return profile;
    }

    public void Save()
    {
        var values = ApplicationData.Current.LocalSettings.Values;
        values[KeyBusinessName] = BusinessName;
        values[KeyAddress] = Address;
        values[KeyEmail] = Email;
        values[KeyPhone] = Phone;
        values[KeyTaxRate] = TaxRate;
    }
}
