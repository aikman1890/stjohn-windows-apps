namespace ClaimPilot.Models;

/// <summary>One of the prominent honest-notes shown on the Civil Rights page.</summary>
public sealed class HonestNote
{
    public HonestNote(string title, string body)
    {
        Title = title;
        Body = body;
    }

    public string Title { get; }
    public string Body { get; }
}
