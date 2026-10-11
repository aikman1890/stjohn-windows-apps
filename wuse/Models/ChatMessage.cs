namespace Wuse.Models;

/// <summary>
/// A single message in a conversation. Role is "user" or "assistant".
/// </summary>
public sealed class ChatMessage
{
    public string Role { get; set; } = "user";
    public string Text { get; set; } = string.Empty;
    public DateTime At { get; set; } = DateTime.Now;
}
