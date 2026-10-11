using BizPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BizPilot.Views;

public sealed partial class InvoicesPage : Page
{
    public InvoicesViewModel ViewModel { get; }

    public InvoicesPage()
    {
        InitializeComponent();
        ViewModel = new InvoicesViewModel
        {
            OwnerWindow = ((App)Application.Current).MainWindow
        };
        DataContext = ViewModel;
    }
}
