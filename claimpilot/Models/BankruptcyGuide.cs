namespace ClaimPilot.Models;

/// <summary>One plain-English bankruptcy chapter guide, made of titled sections.</summary>
public sealed class BankruptcyGuide
{
    public BankruptcyGuide(string title, string intro, IReadOnlyList<BankruptcySection> sections)
    {
        Title = title;
        Intro = intro;
        Sections = sections;
    }

    public string Title { get; }
    public string Intro { get; }
    public IReadOnlyList<BankruptcySection> Sections { get; }
}

/// <summary>One titled section inside a bankruptcy guide.</summary>
public sealed class BankruptcySection
{
    public BankruptcySection(string heading, string body)
    {
        Heading = heading;
        Body = body;
    }

    public string Heading { get; }
    public string Body { get; }
}
