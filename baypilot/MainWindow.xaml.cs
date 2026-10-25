using BayPilot.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BayPilot;

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
            "board" => typeof(BoardPage),
            "customers" => typeof(CustomersPage),
            "vehicles" => typeof(VehiclesPage),
            "estimates" => typeof(EstimatesPage),
            "approvals" => typeof(ApprovalsPage),
            "declined" => typeof(DeclinedPage),
            "invoices" => typeof(InvoicesPage),
            "reminders" => typeof(RemindersPage),
            "settings" => typeof(SettingsPage),
            _ => typeof(DashboardPage),
        };
        if (ContentFrame.CurrentSourcePageType != page)
            ContentFrame.Navigate(page);
    }
}
