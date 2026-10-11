using System.Runtime.InteropServices;
using DayPilot.Services;
using DayPilot.ViewModels;
using DayPilot.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace DayPilot;

/// <summary>
/// Main window: NavigationView shell plus the Ctrl+Shift+D global hotkey
/// (QuickCapture) that brings DayPilot forward and jumps to Notes.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    private const int HotkeyId = 0x0D41;
    private const uint ModControlShift = 0x0006; // MOD_CONTROL | MOD_SHIFT
    private const uint VkD = 0x44;               // 'D'
    private const uint WmHotkey = 0x0312;
    private const int SwRestore = 9;

    private readonly nint _hwnd;
    private readonly SUBCLASSPROC _subclassProc; // kept alive for the window's lifetime
    private readonly Dictionary<string, NavigationViewItem> _navItems;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool SetWindowSubclass(nint hWnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass, nuint dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool RemoveWindowSubclass(nint hWnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass);

    [DllImport("comctl32.dll")]
    private static extern nint DefSubclassProc(nint hWnd, uint uMsg, nint wParam, nint lParam);

    private delegate nint SUBCLASSPROC(nint hWnd, uint uMsg, nint wParam, nint lParam, nuint uIdSubclass, nuint dwRefData);

    public MainWindow()
    {
        this.InitializeComponent();
        ((FrameworkElement)this.Content).DataContext = ViewModel;

        _hwnd = WindowNative.GetWindowHandle(this);
        _subclassProc = SubclassProc;

        _navItems = new Dictionary<string, NavigationViewItem>
        {
            ["briefing"] = BriefingNavItem,
            ["notes"] = NotesNavItem,
            ["reminders"] = RemindersNavItem,
            ["settings"] = SettingsNavItem,
        };

        ViewModel.NavigateRequested += OnNavigateRequested;
        this.Closed += OnClosed;

        RegisterGlobalHotKey();
        NotificationService.Instance.StartReminderMonitor(new ReminderStore());
        _ = AiService.Instance.InitializeAsync();

        ViewModel.SelectPage("briefing");
    }

    private void OnNavigateRequested(object? sender, Type pageType)
    {
        ContentFrame.Navigate(pageType);
        if (_navItems.TryGetValue(ViewModel.SelectedPageTag, out var item))
            NavView.SelectedItem = item;
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item
            && item.Tag is string tag
            && !string.Equals(tag, ViewModel.SelectedPageTag, StringComparison.Ordinal))
        {
            ViewModel.SelectPage(tag);
        }
    }

    private void RegisterGlobalHotKey()
    {
        if (!RegisterHotKey(_hwnd, HotkeyId, ModControlShift, VkD))
        {
            // Another application already owns Ctrl+Shift+D. DayPilot keeps
            // working normally; only the global hotkey is unavailable.
            return;
        }
        SetWindowSubclass(_hwnd, _subclassProc, 1, 0);
    }

    private nint SubclassProc(nint hWnd, uint uMsg, nint wParam, nint lParam, nuint uIdSubclass, nuint dwRefData)
    {
        if (uMsg == WmHotkey && wParam == (nint)HotkeyId)
        {
            DispatcherQueue.TryEnqueue(ActivateFromHotkey);
            return nint.Zero;
        }
        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    private void ActivateFromHotkey()
    {
        ShowWindow(_hwnd, SwRestore);
        SetForegroundWindow(_hwnd);
        ViewModel.SelectPage("notes");
        if (ContentFrame.Content is NotesPage notesPage)
            notesPage.FocusTitleBox();
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        RemoveWindowSubclass(_hwnd, _subclassProc, 1);
        UnregisterHotKey(_hwnd, HotkeyId);
    }
}
