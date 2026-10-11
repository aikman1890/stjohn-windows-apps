namespace ClaimPilot.Models;

/// <summary>One titled section of plain-English legal information.</summary>
public sealed class GuideSection
{
    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public GuideSection()
    {
    }

    public GuideSection(string title, string body)
    {
        Title = title;
        Body = body;
    }
}
