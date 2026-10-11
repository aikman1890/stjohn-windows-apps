using System;
using BizPilot.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BizPilot;

public sealed partial class MainWindow : Window
{
    private readonly object? _shellContent;
    private readonly StJohn.Licensing.LicenseManager _licensing;
    private bool _activatedThisSession;

    public MainWindow()
    {
        InitializeComponent();
        _shellContent = this.Content;
        _licensing = Licensing.AppLicense.Manager;

        // License gate: no valid key -> lockout page INSTEAD of the shell.
        if (!_licensing.HasAccess())
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
        Title = "BizPilot";
        // Selecting the first item raises SelectionChanged, which navigates to DashboardPage.
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    private void NavigateTo(string tag)
    {
        Type? pageType = tag switch
        {
            "dashboard" => typeof(DashboardPage),
            "clients" => typeof(ClientsPage),
            "invoices" => typeof(InvoicesPage),
            "expenses" => typeof(ExpensesPage),
            "aitools" => typeof(AiToolsPage),
            // Windows Ink support (added 2026-10-10; own block to avoid merge conflicts with licensing work).
            "inknotes" => typeof(InkNotesPage),
            "settings" => typeof(SettingsPage),
            _ => null
        };

        if (pageType is not null && ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
