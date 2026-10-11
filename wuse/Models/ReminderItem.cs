namespace Wuse.Models;

/// <summary>
/// A single reminder with a due date/time, stored in wuse's local folder.
/// </summary>
public sealed class ReminderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Text { get; set; } = string.Empty;

    public DateTime DueAt { get; set; } = DateTime.Now.AddHours(1);

    public bool IsDone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
