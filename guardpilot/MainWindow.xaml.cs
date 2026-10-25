using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GuardPilot
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(Views.DashboardPage));
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not NavigationViewItem item) return;
            var type = (item.Tag as string) switch
            {
                "sites" => typeof(Views.SitesPage),
                "orders" => typeof(Views.OrdersPage),
                "guards" => typeof(Views.GuardsPage),
                "schedule" => typeof(Views.SchedulePage),
                "incidents" => typeof(Views.IncidentsPage),
                "dar" => typeof(Views.DarPage),
                "billing" => typeof(Views.BillingPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
