using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WellPilot
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
                "wells" => typeof(Views.WellsPage),
                "owners" => typeof(Views.OwnersPage),
                "decks" => typeof(Views.DecksPage),
                "production" => typeof(Views.ProductionPage),
                "expenses" => typeof(Views.ExpensesPage),
                "close" => typeof(Views.ClosePage),
                "suspense" => typeof(Views.SuspensePage),
                "calendar" => typeof(Views.CalendarPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
