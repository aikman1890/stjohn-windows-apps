namespace CharterPilot.Models;

public sealed class BrokerClient
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    /// <summary>The assistant/booker who actually books - often the real contact.</summary>
    public string BookerName { get; set; } = string.Empty;
    public string BookerContact { get; set; } = string.Empty;
    /// <summary>Preferences: aircraft class, cabin needs, catering, FBO, ground transport.</summary>
    public string Preferences { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
