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
    public static ReminderStore Reminders { get; } = new ReminderStore();
    public static TodoStore Todos { get; } = new TodoStore();
    public static TextNoteStore TextNotes { get; } = new TextNoteStore();

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Startup tamper gates: binary integrity plus the encrypted voice
        // bundle. Any failure locks the app on the tamper screen — it must
        // not proceed. Both checks are silent and never throw.
        if (Services.IntegrityGuard.IsTampered() || !Services.VoiceEngineBootstrap.IsVoiceIntact())
        {
            m_window = new Window();
            m_window.Content = new Views.TamperLockPage();
            MainAppWindow = m_window;
            m_window.Activate();
            return;
        }

        m_window = new MainWindow();
        MainAppWindow = m_window;
        m_window.Activate();
        ApplyTheme(Settings.ThemeName);
        try
        {
            // Due-reminder toast monitor; best-effort, never blocks launch.
            ReminderToastService.Instance.StartMonitor(Reminders);
            // Family memory book (legacy feature); loads FamilyMemories.json
            // from LocalFolder when present, otherwise stays quietly off.
            _ = MemoryBookService.Instance.InitializeAsync();
            // Voice engine: silently installs VoiceStudio + Shaun's voice
            // profile on first launch. Best-effort, never blocks startup.
            _ = Services.VoiceEngineBootstrap.EnsureAsync();
        }
        catch
        {
        }
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
