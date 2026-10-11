using BizPilot.Services;
using Microsoft.UI.Xaml;
using QuestPDF.Infrastructure;

namespace BizPilot;

public partial class App : Application
{
    public Window MainWindow { get; private set; } = null!;
    public DbService Db { get; } = new DbService();
    public AiService Ai { get; } = new AiService();

    public App()
    {
        InitializeComponent();
        QuestPDF.Settings.License = LicenseType.Community;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Startup tamper gate: binary integrity. Any failure locks the app
        // on the tamper screen instead of the main window.
        if (Services.IntegrityGuard.IsTampered())
        {
            var lockWindow = new Window();
            lockWindow.Content = new Microsoft.UI.Xaml.Controls.TextBlock { Text = "Tampered" };
            MainWindow = lockWindow;
            MainWindow.Activate();
            return;
        }

        MainWindow = new MainWindow();
        _ = Ai.InitializeAsync();
        MainWindow.Activate();
    }
}
