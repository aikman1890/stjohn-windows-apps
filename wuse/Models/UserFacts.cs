namespace Wuse.Models;

/// <summary>
/// What the assistant calls itself, plus the facts the user asked it to remember.
/// Persisted through SettingsService.
/// </summary>
public sealed class UserFacts
{
    public string AssistantName { get; set; } = "wuse";
    public List<string> Facts { get; set; } = new();
}
