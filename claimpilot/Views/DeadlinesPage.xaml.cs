using ClaimPilot.Models;
using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ClaimPilot.Views;

/// <summary>Deadlines page.</summary>
public sealed partial class DeadlinesPage : Page
{
    public DeadlinesViewModel ViewModel { get; } = new();

    public DeadlinesPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = ViewModel.LoadAsync();
    }

    private async void DeleteDeadline_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is DeadlineItem item)
        {
            await ViewModel.DeleteDeadlineAsync(item);
        }
    }

    private async void DoneCheck_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as CheckBox)?.DataContext is DeadlineItem item)
        {
            await ViewModel.SaveDeadlineAsync(item);
        }
    }
}
