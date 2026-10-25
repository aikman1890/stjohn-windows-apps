using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace StablePilot
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
                "horses" => typeof(Views.HorsesPage),
                "care" => typeof(Views.CarePage),
                "board" => typeof(Views.BoardPage),
                "health" => typeof(Views.HealthPage),
                "travel" => typeof(Views.TravelPage),
                "billing" => typeof(Views.BillingPage),
                "providers" => typeof(Views.ProvidersPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
