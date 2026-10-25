namespace EstatePilot.Models;

public sealed class Property
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    /// <summary>Primary / Seasonal.</summary>
    public string Type { get; set; } = "Primary";
    /// <summary>Key systems, free-form: HVAC, generator, pool, elevator, security (that a system exists + who services it).</summary>
    public string SystemsNote { get; set; } = string.Empty;
    /// <summary>
    /// Access as POINTERS ONLY ("keys held by the house manager", "alarm serviced by X").
    /// By design there is NO field anywhere in this app for alarm codes, safe
    /// combinations, gate codes, or passwords - it never holds the keys to the house.
    /// </summary>
    public string AccessPointer { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
