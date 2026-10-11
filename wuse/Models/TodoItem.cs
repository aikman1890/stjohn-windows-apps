namespace Wuse.Models;

/// <summary>
/// A single todo on wuse's task list, stored in the app's local folder.
/// </summary>
public sealed class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Text { get; set; } = string.Empty;

    public bool IsDone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
