using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Wuse.Models;
using Wuse.ViewModels;

namespace Wuse.Views;

public sealed partial class ConversationsPage : Page
{
    public ConversationsViewModel ViewModel { get; }

    private bool _initialized;

    public ConversationsPage()
    {
        ViewModel = new ConversationsViewModel(App.Store);
        this.InitializeComponent();
        ViewModel.NavigateToChat = conversation =>
            ((MainWindow)App.MainAppWindow!).NavigateToChat(conversation);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (!_initialized)
        {
            _initialized = true;
            await ViewModel.InitializeAsync();
        }
        else
        {
            ViewModel.RefreshList();
        }
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Conversation conversation)
            ViewModel.Open(conversation);
    }

    private async void RenameButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Conversation conversation)
            await ViewModel.RenameAsync(conversation, PromptForTitleAsync);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Conversation conversation)
            await ViewModel.DeleteAsync(conversation);
    }

    private async Task<string?> PromptForTitleAsync(string currentTitle)
    {
        var textBox = new TextBox { Text = currentTitle, MinWidth = 280 };
        textBox.SelectAll();
        var dialog = new ContentDialog
        {
            Title = "Rename conversation",
            Content = textBox,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot,
        };
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? textBox.Text : null;
    }
}
