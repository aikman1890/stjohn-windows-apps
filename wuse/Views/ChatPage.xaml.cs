using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using Wuse.Models;
using Wuse.ViewModels;

namespace Wuse.Views;

public sealed partial class ChatPage : Page
{
    public ChatViewModel ViewModel { get; }

    private bool _initialized;

    public ChatPage()
    {
        ViewModel = new ChatViewModel(App.Settings, App.Ai, App.Store, App.Quick);
        this.InitializeComponent();
        ViewModel.Messages.CollectionChanged += OnMessagesChanged;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Conversation conversation)
            ViewModel.LoadConversation(conversation);
        else
            ViewModel.StartNewConversation();

        if (!_initialized)
        {
            _initialized = true;
            await ViewModel.InitializeAsync();
        }
    }

    private void OnMessagesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (MessageList.Items.Count > 0)
            MessageList.ScrollIntoView(MessageList.Items[MessageList.Items.Count - 1]);
    }

    private void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.SendCommand.CanExecute(null))
        {
            ViewModel.SendCommand.Execute(null);
            e.Handled = true;
        }
    }
}
