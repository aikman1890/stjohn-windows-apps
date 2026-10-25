using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace YachtPilot
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
                "vessels" => typeof(Views.VesselsPage),
                "crew" => typeof(Views.CrewPage),
                "maintenance" => typeof(Views.MaintenancePage),
                "calendar" => typeof(Views.CalendarPage),
                "expenses" => typeof(Views.ExpensesPage),
                "vendors" => typeof(Views.VendorsPage),
                "activity" => typeof(Views.ActivityPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
