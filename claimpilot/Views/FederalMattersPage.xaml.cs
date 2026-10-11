using System;
using ClaimPilot.Services;
using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>Federal Matters page: state vs federal, federal process, civil basics. Gated.</summary>
public sealed partial class FederalMattersPage : Page
{
    public FederalMattersViewModel ViewModel { get; } = new();

    public FederalMattersPage()
    {
        InitializeComponent();
    }

    private async void BuyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(LegalMattersGate.BuyMailTo));
    }

    private void ActivateKey_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        KeyStatusText.Text = string.Empty;
        if (LegalMattersGate.TryActivate(LegalMattersKeyBox.Text, LegalMattersEmailBox.Text, out var error))
        {
            KeyStatusText.Text = "Legal Matters unlocked.";
            Frame.Navigate(GetType());
        }
        else
        {
            KeyStatusText.Text = error;
        }
    }

    private void CourtFinderButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        (App.MainWindow as MainWindow)?.NavigateToTag("courtfinder");
    }
}
