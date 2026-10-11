using BizPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BizPilot.Views;

public sealed partial class ExpensesPage : Page
{
    public ExpensesViewModel ViewModel { get; }

    public ExpensesPage()
    {
        InitializeComponent();
        ViewModel = new ExpensesViewModel
        {
            OwnerWindow = ((App)Application.Current).MainWindow
        };
        DataContext = ViewModel;
    }
}
