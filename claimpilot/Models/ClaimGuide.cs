namespace ClaimPilot.Models;

/// <summary>A plain-English guide to one type of federal civil-rights claim.</summary>
public sealed class ClaimGuide
{
    public ClaimGuide(string title, string whatItIs, string whatPlaintiffMustShow, string commonDefenses)
    {
        Title = title;
        WhatItIs = whatItIs;
        WhatPlaintiffMustShow = whatPlaintiffMustShow;
        CommonDefenses = commonDefenses;
    }

    public string Title { get; }
    public string WhatItIs { get; }
    public string WhatPlaintiffMustShow { get; }
    public string CommonDefenses { get; }
}
