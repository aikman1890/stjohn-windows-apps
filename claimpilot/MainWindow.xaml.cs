using ClaimPilot.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot;

/// <summary>
/// Main window hosting the NavigationView and content Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private bool _suppressSelectionChanged;

    private readonly object? _shellContent;
    private readonly StJohn.Licensing.LicenseManager _licensing;
    private bool _activatedThisSession;

    public MainWindow()
    {
        InitializeComponent();
        _shellContent = this.Content;

        // Creator unlock (7 taps on About) bypasses the base license gate.
        // Otherwise: no valid base key -> lockout page INSTEAD of the shell.
        _licensing = Licensing.AppLicense.Base;
        if (!Licensing.AppLicense.CreatorUnlocked && !_licensing.HasAccess())
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
    /// Never runs during a trial, with a valid license, or creator-unlocked.
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
        Title = "ClaimPilot";
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        NavigateToTag("dashboard");
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_suppressSelectionChanged)
        {
            return;
        }

        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            NavigateToTag(tag);
        }
    }

    /// <summary>Navigates the content frame to the page for the given tag.</summary>
    public void NavigateToTag(string tag)
    {
        Type? pageType = tag switch
        {
            "dashboard" => typeof(DashboardPage),
            "violations" => typeof(ViolationSpotterPage),
            "letters" => typeof(LetterBuilderPage),
            "worksheet" => typeof(WorksheetPage),
            "civilrights" => typeof(ClaimGuidesPage),
            "courtfinder" => typeof(CourtFinderPage),
            "bankruptcy" => typeof(BankruptcyPage),
            "traffictickets" => typeof(TrafficTicketsPage),
            "criminalcases" => typeof(CriminalCasesPage),
            "federalmatters" => typeof(FederalMattersPage),
            "courtguide" => typeof(CourtGuidePage),
            "casebuilder" => typeof(CaseBuilderPage),
            "deadlines" => typeof(DeadlinesPage),
            "vault" => typeof(EvidenceVaultPage),
            // Windows Ink support (added 2026-10-10; own block to avoid merge conflicts with licensing work).
            "inknotes" => typeof(InkNotesPage),
            "about" => typeof(AboutPage),
            _ => null
        };

        if (pageType is null)
        {
            return;
        }

        ContentFrame.Navigate(pageType);

        _suppressSelectionChanged = true;
        try
        {
            foreach (var menuItem in NavView.MenuItems)
            {
                if (menuItem is NavigationViewItem navItem && navItem.Tag is string itemTag && itemTag == tag)
                {
                    NavView.SelectedItem = navItem;
                    break;
                }
            }
        }
        finally
        {
            _suppressSelectionChanged = false;
        }
    }
}
