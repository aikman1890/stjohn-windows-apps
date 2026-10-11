namespace Wuse.Models;

/// <summary>
/// A quick text note captured by wuse ("take a note ..."), stored locally.
/// </summary>
public sealed class TextNote
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
