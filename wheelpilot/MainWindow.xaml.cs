using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WheelPilot
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
                "students" => typeof(Views.StudentsPage),
                "schedule" => typeof(Views.SchedulePage),
                "cars" => typeof(Views.CarsPage),
                "readiness" => typeof(Views.ReadinessPage),
                "settings" => typeof(Views.SettingsPage),
                _ => typeof(Views.DashboardPage),
            };
            if (ContentFrame.CurrentSourcePageType != type)
                ContentFrame.Navigate(type);
        }
    }
}
