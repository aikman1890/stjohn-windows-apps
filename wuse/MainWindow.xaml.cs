using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Wuse.Views;

namespace Wuse;

public sealed partial class MainWindow : Window
{
    private readonly object? _shellContent;
    private readonly StJohn.Licensing.LicenseManager _licensing;
    private bool _activatedThisSession;

    public MainWindow()
    {
        this.InitializeComponent();
        _shellContent = this.Content;
        _licensing = Licensing.AppLicense.Manager;

        // License gate: no valid key (either tier) -> lockout page INSTEAD of the shell.
        if (!Licensing.AppLicense.HasAppAccess)
        {
            var lockout = new Views.LockoutPage(_licensing);
            lockout.Unlocked += (_, _) =>
            {
                _activatedThisSession = true;
                this.Closed -= OnClosedWhileLocked;
                this.Content = _shellContent;
                ShowMainShell();
            };
            this.Content = lockout;
            this.Closed += OnClosedWhileLocked;
            return;
        }

        ShowMainShell();
    }

    /// <summary>
    /// Exit while locked out without activating: back up the user's work to
    /// Documents, then auto-uninstall the app (per Shaun's requirement).
    /// Never runs with a valid license.
    /// </summary>
    private async void OnClosedWhileLocked(object sender, WindowEventArgs args)
    {
        if (_activatedThisSession)
            return;
        try { await _licensing.BackupSavedWorkAsync(); }
        catch { /* backup is best-effort; uninstall still proceeds */ }
        try { _licensing.WriteUninstallScriptAndLaunch(); }
        catch { /* nothing more can be done while exiting */ }
    }

    private void ShowMainShell()
    {
        this.Title = "wuse";
    }

    /// <summary>
    /// Navigates to the chat page, optionally loading an existing conversation.
    /// A null conversation starts a brand-new chat.
    /// </summary>
    public void NavigateToChat(Models.Conversation? conversation)
    {
        ContentFrame.Navigate(typeof(ChatPage), conversation);
        NavView.SelectedItem = ChatNavItem;
    }

    /// <summary>
    /// Opens the secure vault, optionally with a site name prefilled (from
    /// chat: "save a login for Netflix" → the user types the secrets here,
    /// in masked fields — never in chat).
    /// </summary>
    public void NavigateToVault(string? prefillSite = null)
    {
        if (prefillSite != null)
            ContentFrame.Navigate(typeof(VaultPage), prefillSite);
        else
            ContentFrame.Navigate(typeof(VaultPage));
        NavView.SelectedItem = VaultNavItem;
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(ChatPage));
        NavView.SelectedItem = ChatNavItem;
        _ = ShowFirstRunDialogIfNeededAsync();
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is not NavigationViewItem item)
            return;

        switch (item.Tag as string)
        {
            case "chat":
                ContentFrame.Navigate(typeof(ChatPage));
                break;
            case "conversations":
                ContentFrame.Navigate(typeof(ConversationsPage));
                break;
            case "actions":
                ContentFrame.Navigate(typeof(QuickActionsPage));
                break;
            // Windows Ink support (added 2026-10-10; own block to avoid merge conflicts with licensing work).
            case "inknotes":
                ContentFrame.Navigate(typeof(InkNotesPage));
                break;
            // Secure login vault (added 2026-10-10; own block to avoid merge conflicts).
            case "vault":
                ContentFrame.Navigate(typeof(VaultPage));
                break;
            case "prefs":
                ContentFrame.Navigate(typeof(PreferencesPage));
                break;
            case "about":
                ContentFrame.Navigate(typeof(AboutPage));
                break;
        }
    }

    private async Task ShowFirstRunDialogIfNeededAsync()
    {
        var settings = App.Settings;
        if (settings.HasRun)
            return;
        settings.HasRun = true;

        var name = settings.AssistantName;
        var dialog = new ContentDialog
        {
            Title = $"Hi, I'm {name}!",
            Content = "I run entirely on this PC — no cloud, no accounts, no tracking.\n\n" +
                      "Ask me anything, or type \"help\" to see what I can do. " +
                      "You can rename me anytime in Preferences.",
            CloseButtonText = "Let's go",
            XamlRoot = Content.XamlRoot,
        };
        await dialog.ShowAsync();
    }
}
