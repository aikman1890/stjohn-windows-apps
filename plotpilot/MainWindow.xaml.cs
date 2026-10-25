using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PlotPilot
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
                "register" => typeof(Views.RegisterPage),
                "burials" => typeof(Views.BurialsPage),
                "map" => typeof(Views.MapPage),
                "search" => typeof(Views.SearchPage),
                "reconcile" => typeof(Views.ReconcilePage),
                "worklog" => typeof(Views.WorkLogPage),
                "reports" => typeof(Views.ReportsPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
