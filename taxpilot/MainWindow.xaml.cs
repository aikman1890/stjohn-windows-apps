using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TaxPilot
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
                "clients" => typeof(Views.ClientsPage),
                "engagements" => typeof(Views.EngagementsPage),
                "docs" => typeof(Views.DocsPage),
                "efile" => typeof(Views.EfilePage),
                "contacts" => typeof(Views.ContactsPage),
                "deadlines" => typeof(Views.DeadlinesPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
