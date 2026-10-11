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

    public MainWindow()
    {
        InitializeComponent();
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
            "deadlines" => typeof(DeadlinesPage),
            "vault" => typeof(EvidenceVaultPage),
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
