namespace ClaimPilot.Models;

/// <summary>One U.S. federal judicial district (or a special no-court entry).</summary>
public sealed class DistrictInfo
{
    public DistrictInfo(string districtName, string stateOrTerritory, string divisionCities, string websiteUrl)
    {
        DistrictName = districtName;
        StateOrTerritory = stateOrTerritory;
        DivisionCities = divisionCities;
        WebsiteUrl = new Uri(websiteUrl);
    }

    public string DistrictName { get; }
    public string StateOrTerritory { get; }
    public string DivisionCities { get; }
    public Uri WebsiteUrl { get; }

    /// <summary>Optional note, e.g. explaining a jurisdiction with no federal district court.</summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>True for special entries such as American Samoa (no federal district court).</summary>
    public bool IsSpecialEntry { get; set; }
}
