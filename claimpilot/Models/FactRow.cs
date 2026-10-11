using CommunityToolkit.Mvvm.ComponentModel;

namespace ClaimPilot.Models;

/// <summary>One row of the small-claims facts timeline: a date plus a description.</summary>
public partial class FactRow : ObservableObject
{
    [ObservableProperty]
    private string _date = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;
}
