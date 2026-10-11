using BizPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BizPilot.Views;

public sealed partial class AiToolsPage : Page
{
    public AiToolsViewModel ViewModel { get; }

    public AiToolsPage()
    {
        InitializeComponent();
        ViewModel = new AiToolsViewModel();
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshAiStatus();
    }
}
