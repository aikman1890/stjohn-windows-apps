namespace Wuse.Models;

/// <summary>
/// A saved conversation: an id, a title, a start time, and its messages.
/// </summary>
public sealed class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "New chat";
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public List<ChatMessage> Messages { get; set; } = new();
}
