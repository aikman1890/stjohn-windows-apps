using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Models;

/// <summary>A dashboard navigation card.</summary>
public sealed class NavCard
{
    public NavCard(string title, string description, Symbol iconSymbol, string tag)
    {
        Title = title;
        Description = description;
        IconSymbol = iconSymbol;
        Tag = tag;
    }

    public string Title { get; }
    public string Description { get; }
    public Symbol IconSymbol { get; }
    public string Tag { get; }
}
