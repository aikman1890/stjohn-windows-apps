using System;
using ClaimPilot.Services;
using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>Traffic Tickets page: self-help sections, gated behind the Legal Matters add-on.</summary>
public sealed partial class TrafficTicketsPage : Page
{
    public TrafficTicketsViewModel ViewModel { get; } = new();

    public TrafficTicketsPage()
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
