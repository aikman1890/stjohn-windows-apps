using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>Pro Se Resources page: IFP explainer and per-district court links.</summary>
public sealed partial class ProSeResourcesPage : Page
{
    public ProSeResourcesViewModel ViewModel { get; } = new();

    public ProSeResourcesPage()
    {
        InitializeComponent();
    }
}
