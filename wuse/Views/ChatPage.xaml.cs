using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Wuse.Models;
using Wuse.Services;
using Wuse.ViewModels;

namespace Wuse.Views;

public sealed partial class ChatPage : Page
{
    public ChatViewModel ViewModel { get; }

    private bool _initialized;
    private bool _listening;
    private object? _micOriginalContent;

    public ChatPage()
    {
        ViewModel = new ChatViewModel(
            App.Settings, App.Ai, App.Store, App.Quick,
            App.Reminders, App.Todos, App.TextNotes);
        this.InitializeComponent();
        ViewModel.Messages.CollectionChanged += OnMessagesChanged;
        ViewModel.AssistantReplied += OnAssistantReplied;
        ViewModel.OpenVaultRequested += OnOpenVaultRequested;
    }

    /// <summary>
    /// The router asked for the Vault page (password entry happens in masked
    /// fields there — never in chat).
    /// </summary>
    private void OnOpenVaultRequested(object? sender, string? prefillSite)
    {
        if (App.MainAppWindow is MainWindow window)
            window.NavigateToVault(prefillSite);
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

    /// <summary>
    /// Microphone button: listens once, then drops the transcribed text into
    /// the composer for review. Auto-send stays off so the user can edit first.
    /// </summary>
    private async void MicButton_Click(object sender, RoutedEventArgs e)
    {
        if (_listening)
            return;
        _listening = true;

        try
        {
            MicButton.IsEnabled = false;
            _micOriginalContent = MicButton.Content;
            MicButton.Content = "Listening…";

            var (ok, text, error) = await SpeechService.Instance.BeginRecognitionAsync();

            if (ok && !string.IsNullOrWhiteSpace(text))
            {
                ViewModel.SetVoiceInput(text);
                InputTextBox.Focus(FocusState.Programmatic);
            }
            else
            {
                // Transient note: added to the visible list only, not saved
                // to the conversation store.
                ViewModel.Messages.Add(new ChatMessage
                {
                    Role = "assistant",
                    Text = string.IsNullOrWhiteSpace(error)
                        ? "I didn't catch that — want to try once more?"
                        : error,
                    At = DateTime.Now,
                });
            }
        }
        finally
        {
            if (_micOriginalContent != null)
                MicButton.Content = _micOriginalContent;
            MicButton.IsEnabled = true;
            _listening = false;
        }
    }

    /// <summary>
    /// Speaks the assistant's reply when voice replies are on, or when this
    /// turn was voice-originated. Very long replies are trimmed for speech;
    /// the full text stays on screen.
    /// </summary>
    private async void OnAssistantReplied(object? sender, string text)
    {
        try
        {
            // Always consume the voice-turn flag so it can't leak into a
            // later turn.
            bool voiceTurn = ViewModel.ConsumeVoiceTurn();
            if (!ViewModel.SpeakReplies && !voiceTurn)
                return;
            var speak = text.Length > 600 ? text.Substring(0, 600) : text;
            await SpeechService.Instance.SpeakAsync(speak);
        }
        catch
        {
            // Speech output must never break chat.
        }
    }
}
