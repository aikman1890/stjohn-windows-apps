using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CharterPilot
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
                "quotes" => typeof(Views.QuotesPage),
                "clients" => typeof(Views.ClientsPage),
                "operators" => typeof(Views.OperatorsPage),
                "trips" => typeof(Views.TripsPage),
                "ledger" => typeof(Views.LedgerPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
