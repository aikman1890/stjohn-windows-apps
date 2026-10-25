using Microsoft.UI.Xaml;

namespace GameVault;

/// <summary>
/// Application entry point. Creates and activates the main window on launch.
/// All data stays on this device: local JSON storage only, no network calls.
/// The window instance is exposed so pages can host folder pickers.
/// </summary>
public partial class App : Application
{
    public static Window? WindowInstance { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        WindowInstance = new MainWindow();
        WindowInstance.Activate();
    }
}
