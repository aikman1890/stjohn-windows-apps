using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace EntityPilot
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
                "entities" => typeof(Views.EntitiesPage),
                "deadlines" => typeof(Views.DeadlinesPage),
                "ownership" => typeof(Views.OwnershipPage),
                "intercompany" => typeof(Views.IntercompanyPage),
                "documents" => typeof(Views.DocumentsPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
