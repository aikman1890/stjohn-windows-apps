using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace JobPilot;

/// <summary>
/// Main window: NavigationView shell over the app's pages.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly Dictionary<string, Type> _pages = new()
    {
        ["dashboard"] = typeof(Views.DashboardPage),
        ["applications"] = typeof(Views.ApplicationsPage)
    };

    public MainWindow()
    {
        this.InitializeComponent();
        ContentFrame.Navigate(_pages["dashboard"]);
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item
            && item.Tag is string tag
            && _pages.TryGetValue(tag, out var pageType))
        {
            if (ContentFrame.CurrentSourcePageType != pageType)
                ContentFrame.Navigate(pageType);
        }
    }
}
