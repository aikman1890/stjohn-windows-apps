using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>Violation Spotter page.</summary>
public sealed partial class ViolationSpotterPage : Page
{
    public ViolationSpotterViewModel ViewModel { get; } = new();

    public ViolationSpotterPage()
    {
        InitializeComponent();
    }
}
