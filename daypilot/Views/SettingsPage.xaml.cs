using DayPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace DayPilot.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = new();

    public SettingsPage()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;
    }
}
