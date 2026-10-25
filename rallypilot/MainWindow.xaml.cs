using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace RallyPilot
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
                "packages" => typeof(Views.PackagesPage),
                "quotes" => typeof(Views.QuotesPage),
                "events" => typeof(Views.EventsPage),
                "roster" => typeof(Views.RosterPage),
                "runofday" => typeof(Views.RunOfDayPage),
                "organizers" => typeof(Views.OrganizersPage),
                "reports" => typeof(Views.ReportsPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
