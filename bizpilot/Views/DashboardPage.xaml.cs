using BizPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BizPilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        InitializeComponent();
        ViewModel = new DashboardViewModel();
        DataContext = ViewModel;
    }
}
