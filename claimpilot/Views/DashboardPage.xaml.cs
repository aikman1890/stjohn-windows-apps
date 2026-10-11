using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ClaimPilot.Views;

/// <summary>Dashboard page: module cards plus the disclaimer footer.</summary>
public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; } = new();

    public DashboardPage()
    {
        InitializeComponent();
    }

    private void CardButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string tag })
        {
            ViewModel.Navigate(tag);
        }
    }
}
