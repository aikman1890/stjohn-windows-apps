using System;
using ClaimPilot.Models;
using ClaimPilot.Services;
using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>Case Builder page: guided assembly of a case file. Gated behind the Legal Matters add-on.</summary>
public sealed partial class CaseBuilderPage : Page
{
    public CaseBuilderViewModel ViewModel { get; } = new();

    public CaseBuilderPage()
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

    private void RemovePartyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is string party)
        {
            ViewModel.RemoveParty(party);
        }
    }

    private void RemoveEventButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is TimelineEvent item)
        {
            ViewModel.RemoveTimelineEvent(item);
        }
    }

    private void RemoveClaimButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is string claim)
        {
            ViewModel.RemoveClaim(claim);
        }
    }

    private void RemoveEvidenceButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is ChecklistItem item)
        {
            ViewModel.RemoveEvidence(item);
        }
    }
}
