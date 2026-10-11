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
        LicenseStatusText.Text = Licensing.AppLicense.Manager.GetStatusText();
    }

    private void ActivateLicense_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        LicenseResultText.Text = string.Empty;
        if (Licensing.AppLicense.Manager.TryActivate(LicenseKeyBox.Text, LicenseEmailBox.Text, out var error))
        {
            LicenseResultText.Text = "Activated — thank you!";
            LicenseStatusText.Text = Licensing.AppLicense.Manager.GetStatusText();
        }
        else
        {
            LicenseResultText.Text = error;
        }
    }
}
