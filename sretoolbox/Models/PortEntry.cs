namespace SreToolbox.Models;

/// <summary>
/// One well-known port: number, transport, service name, and what it is for.
/// </summary>
public sealed class PortEntry
{
    public int Port { get; set; }
    public string Transport { get; set; } = "tcp";
    public string Service { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string DisplayLine => $"{Port}/{Transport} - {Service}";

    public string SubLine => Description;
}
