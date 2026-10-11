using System.Collections.ObjectModel;
using ClaimPilot.Data;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClaimPilot.ViewModels;

/// <summary>
/// Court Finder: searchable list of all 94 federal judicial districts plus the
/// American Samoa special entry (no federal district court — never invented).
/// </summary>
public partial class CourtFinderViewModel : ObservableObject
{
    public ObservableCollection<DistrictInfo> FilteredDistricts { get; } = new();

    [ObservableProperty]
    private string _districtCountText = string.Empty;

    public string IntroText =>
        "Search by state, territory, or district name. Every card links to that court's official website. " +
        "Always confirm filing rules on the court's own site — ClaimPilot does not file anything.";

    public string DisclaimerText => LegalText.Disclaimer;

    public CourtFinderViewModel()
    {
        // Hard self-check: the data layer must expose exactly 94 districts.
        var count = Districts.Count;
        CountNote = count == 94
            ? "Data check passed: 94 federal judicial districts loaded."
            : $"DATA WARNING: expected 94 districts but found {count}.";
        ApplyFilter(string.Empty);
    }

    public string CountNote { get; }

    public void SetSearchText(string text)
    {
        ApplyFilter(text ?? string.Empty);
    }

    private void ApplyFilter(string query)
    {
        var q = query.Trim();
        var matches = string.IsNullOrEmpty(q)
            ? Districts.Searchable
            : Districts.Searchable.Where(d =>
                d.StateOrTerritory.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                d.DistrictName.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        FilteredDistricts.Clear();
        foreach (var district in matches)
        {
            FilteredDistricts.Add(district);
        }

        DistrictCountText = $"Showing {FilteredDistricts.Count} of {Districts.Searchable.Count} entries ({Districts.Count} federal districts).";
    }
}
