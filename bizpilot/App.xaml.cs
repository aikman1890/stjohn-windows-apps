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
        MainWindow = new MainWindow();
        _ = Ai.InitializeAsync();
        MainWindow.Activate();
    }
}
