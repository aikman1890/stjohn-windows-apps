using System;
using ClaimPilot.Services;
using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>Criminal Cases page: rights, case stages, bail, after-arrest guidance. Gated.</summary>
public sealed partial class CriminalCasesPage : Page
{
    public CriminalCasesViewModel ViewModel { get; } = new();

    public CriminalCasesPage()
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
}
