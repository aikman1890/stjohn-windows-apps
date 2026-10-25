using KennelPilot.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KennelPilot;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        Nav.SelectedItem = Nav.MenuItems[0];
        ContentFrame.Navigate(typeof(BoardPage));
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item) return;
        var tag = item.Tag as string;
        var page = tag switch
        {
            "board" => typeof(BoardPage),
            "owners" => typeof(OwnersPage),
            "pets" => typeof(PetsPage),
            "care" => typeof(CareLogPage),
            "vaccines" => typeof(VaccineWatchPage),
            "billing" => typeof(BillingPage),
            "settings" => typeof(SettingsPage),
            _ => typeof(BoardPage),
        };
        if (ContentFrame.CurrentSourcePageType != page)
            ContentFrame.Navigate(page);
    }
}
