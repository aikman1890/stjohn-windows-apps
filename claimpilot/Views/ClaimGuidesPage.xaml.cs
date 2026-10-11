using ClaimPilot.Models;
using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>Civil Rights page: claim guides, honest notes, AI explainer.</summary>
public sealed partial class ClaimGuidesPage : Page
{
    public ClaimGuidesViewModel ViewModel { get; } = new();

    public ClaimGuidesPage()
    {
        InitializeComponent();
    }

    private async void ExplainButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is ClaimGuide guide)
        {
            await ViewModel.ExplainGuideAsync(guide);
        }
    }
}
