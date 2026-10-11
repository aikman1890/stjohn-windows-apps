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
        // Startup tamper gates: binary integrity plus the encrypted voice
        // bundle. Any failure locks the app on the tamper screen.
        if (Services.IntegrityGuard.IsTampered() || !Services.VoiceEngineBootstrap.IsVoiceIntact())
        {
            _window = new Window();
            _window.Content = new Views.TamperLockPage();
            _window.Activate();
            return;
        }

        _window = new MainWindow();
        _window.Activate();
        // Voice engine: silently installs VoiceStudio + Shaun's voice
        // profile on first launch. Best-effort, never blocks startup.
        _ = Services.VoiceEngineBootstrap.EnsureAsync();
    }
}
