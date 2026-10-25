using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HuntPilot
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
                "species" => typeof(Views.SpeciesPage),
                "hunters" => typeof(Views.HuntersPage),
                "hunts" => typeof(Views.HuntsPage),
                "harvest" => typeof(Views.HarvestPage),
                "trophy" => typeof(Views.TrophyPage),
                "lodge" => typeof(Views.LodgePage),
                "reports" => typeof(Views.ReportsPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
