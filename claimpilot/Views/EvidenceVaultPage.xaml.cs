using ClaimPilot.Models;
using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ClaimPilot.Views;

/// <summary>Evidence Vault page.</summary>
public sealed partial class EvidenceVaultPage : Page
{
    public EvidenceVaultViewModel ViewModel { get; } = new();

    public EvidenceVaultPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = ViewModel.LoadAsync();
    }

    private async void DeleteEvidence_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is EvidenceItem item)
        {
            await ViewModel.DeleteEvidenceAsync(item);
        }
    }

    private async void DeleteTimelineEvent_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is TimelineEvent item)
        {
            await ViewModel.DeleteTimelineEventAsync(item);
        }
    }
}
