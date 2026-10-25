using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace EstatePilot
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
                "properties" => typeof(Views.PropertiesPage),
                "services" => typeof(Views.ServicesPage),
                "people" => typeof(Views.PeoplePage),
                "manual" => typeof(Views.ManualPage),
                "jobs" => typeof(Views.JobsPage),
                "checklists" => typeof(Views.ChecklistsPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
