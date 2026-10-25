namespace BayPilot.Models;

/// <summary>
/// One shop customer: contact details. Their vehicles live in Vehicle
/// records that point back to this customer.
/// </summary>
public sealed class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => Name;

    public string SubLine =>
        (string.IsNullOrWhiteSpace(Phone) ? "" : Phone) +
        (string.IsNullOrWhiteSpace(Email) ? "" : $" - {Email}");
}
