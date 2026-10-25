using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgreementPilot
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
                "customers" => typeof(Views.CustomersPage),
                "equipment" => typeof(Views.EquipmentPage),
                "agreements" => typeof(Views.AgreementsPage),
                "tuneups" => typeof(Views.TuneUpsPage),
                "visits" => typeof(Views.VisitsPage),
                "watch" => typeof(Views.WatchPage),
                "profit" => typeof(Views.ProfitPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
