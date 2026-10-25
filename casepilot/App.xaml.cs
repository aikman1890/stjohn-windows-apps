using Microsoft.UI.Xaml;

namespace CasePilot;

/// <summary>
/// Application entry point. Creates and activates the main window on launch.
/// All data stays on this device: local JSON storage only, no network calls.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
