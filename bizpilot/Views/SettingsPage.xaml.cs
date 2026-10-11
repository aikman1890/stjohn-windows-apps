using BizPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BizPilot.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        InitializeComponent();
        ViewModel = new SettingsViewModel();
        DataContext = ViewModel;
    }
}
