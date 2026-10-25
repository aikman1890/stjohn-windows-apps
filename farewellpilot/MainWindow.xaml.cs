using FarewellPilot.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FarewellPilot;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        Nav.SelectedItem = Nav.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item) return;
        var tag = item.Tag as string;
        var page = tag switch
        {
            "dashboard" => typeof(DashboardPage),
            "cases" => typeof(CasesPage),
            "clocks" => typeof(ClocksPage),
            "contracts" => typeof(ContractsPage),
            "preneed" => typeof(PreneedPage),
            "aftercare" => typeof(AftercarePage),
            "reports" => typeof(ReportsPage),
            "settings" => typeof(SettingsPage),
            _ => typeof(DashboardPage),
        };
        if (ContentFrame.CurrentSourcePageType != page)
            ContentFrame.Navigate(page);
    }
}
