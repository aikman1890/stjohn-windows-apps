using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>
/// Full-screen lockout shown when the integrity guard or the voice-bundle
/// check detects tampering. Nothing on this page unlocks the app — the only
/// way back in is a clean reinstall.
/// </summary>
public sealed partial class TamperLockPage : Page
{
    public TamperLockPage()
    {
        this.InitializeComponent();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Application.Current.Exit();
        }
        catch
        {
        }
    }
}
