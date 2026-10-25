using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GalleryPilot
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
                "inventory" => typeof(Views.InventoryPage),
                "consignments" => typeof(Views.ConsignmentsPage),
                "sales" => typeof(Views.SalesPage),
                "loans" => typeof(Views.LoansPage),
                "collectors" => typeof(Views.CollectorsPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
