using Microsoft.UI.Xaml;
using Wuse.Services;

namespace Wuse;

public partial class App : Application
{
    private Window? m_window;

    public static Window? MainAppWindow { get; private set; }

    // Shared services, one instance for the whole app lifetime.
    public static SettingsService Settings { get; } = new SettingsService();
    public static AiService Ai { get; } = new AiService();
    public static ConversationStore Store { get; } = new ConversationStore();
    public static QuickActionService Quick { get; } = new QuickActionService();

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        MainAppWindow = m_window;
        m_window.Activate();
        ApplyTheme(Settings.ThemeName);
    }

    /// <summary>
    /// Applies the saved theme to the window content. WinUI 3 has no
    /// Window.Current, so the root Frame/element hosted by the window is themed directly.
    /// </summary>
    public static void ApplyTheme(string themeName)
    {
        try
        {
            if (MainAppWindow?.Content is FrameworkElement root)
            {
                root.RequestedTheme = themeName switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default,
                };
            }
        }
        catch
        {
            // Theme is cosmetic; never let it break the app.
        }
    }
}
