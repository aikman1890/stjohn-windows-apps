using Microsoft.UI.Xaml;

namespace DayPilot;

/// <summary>
/// Application entry point. Creates and activates the main window on launch.
/// </summary>
public partial class App : Application
{
    private MainWindow? _window;

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
