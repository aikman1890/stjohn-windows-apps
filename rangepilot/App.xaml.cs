using Microsoft.UI.Xaml;

namespace RangePilot;

/// <summary>
/// Application entry point. Creates and activates the main window on launch.
/// All data stays on this device: local JSON storage only, no network calls.
/// </summary>
public partial class App : Application
{
    public static Window? WindowInstance { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        WindowInstance = new MainWindow();
        WindowInstance.Activate();
        await Services.SampleSeed.SeedIfFirstRunAsync();
    }
}
