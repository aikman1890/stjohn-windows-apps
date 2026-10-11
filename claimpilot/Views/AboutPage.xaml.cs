using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>About page: full disclaimer, version, data handling.</summary>
public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel { get; } = new();

    public AboutPage()
    {
        InitializeComponent();
    }
}
