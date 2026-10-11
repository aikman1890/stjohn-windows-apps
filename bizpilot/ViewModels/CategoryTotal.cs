using CommunityToolkit.Mvvm.ComponentModel;

namespace BizPilot.ViewModels;

public partial class CategoryTotal : ObservableObject
{
    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private double total;

    [ObservableProperty]
    private double barWidth;
}
