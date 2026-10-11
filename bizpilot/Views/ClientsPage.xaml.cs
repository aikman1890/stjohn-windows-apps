using BizPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BizPilot.Views;

public sealed partial class ClientsPage : Page
{
    public ClientsViewModel ViewModel { get; }

    public ClientsPage()
    {
        InitializeComponent();
        ViewModel = new ClientsViewModel();
        DataContext = ViewModel;
    }
}
