using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Wuse.Views;

namespace Wuse;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "wuse";
    }

    /// <summary>
    /// Navigates to the chat page, optionally loading an existing conversation.
    /// A null conversation starts a brand-new chat.
    /// </summary>
    public void NavigateToChat(Models.Conversation? conversation)
    {
        ContentFrame.Navigate(typeof(ChatPage), conversation);
        NavView.SelectedItem = ChatNavItem;
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(ChatPage));
        NavView.SelectedItem = ChatNavItem;
        _ = ShowFirstRunDialogIfNeededAsync();
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedArgs args)
    {
        if (args.InvokedItemContainer is not NavigationViewItem item)
            return;

        switch (item.Tag as string)
        {
            case "chat":
                ContentFrame.Navigate(typeof(ChatPage));
                break;
            case "conversations":
                ContentFrame.Navigate(typeof(ConversationsPage));
                break;
            case "actions":
                ContentFrame.Navigate(typeof(QuickActionsPage));
                break;
            case "prefs":
                ContentFrame.Navigate(typeof(PreferencesPage));
                break;
            case "about":
                ContentFrame.Navigate(typeof(AboutPage));
                break;
        }
    }

    private async Task ShowFirstRunDialogIfNeededAsync()
    {
        var settings = App.Settings;
        if (settings.HasRun)
            return;
        settings.HasRun = true;

        var name = settings.AssistantName;
        var dialog = new ContentDialog
        {
            Title = $"Hi, I'm {name}!",
            Content = "I run entirely on this PC — no cloud, no accounts, no tracking.\n\n" +
                      "Ask me anything, or type \"help\" to see what I can do. " +
                      "You can rename me anytime in Preferences.",
            CloseButtonText = "Let's go",
            XamlRoot = Content.XamlRoot,
        };
        await dialog.ShowAsync();
    }
}
