using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wuse.Models;
using Wuse.Services;

namespace Wuse.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly AiService _ai;
    private readonly ConversationStore _store;
    private readonly QuickActionService _quick;

    private Conversation _conversation = new();
    private bool _conversationSaved;
    private bool _initialized;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string inputText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private bool isBusy;

    [ObservableProperty]
    private bool isAiUnavailable;

    [ObservableProperty]
    private string aiStatusMessage = "Checking on-device AI…";

    public ChatViewModel(SettingsService settings, AiService ai, ConversationStore store, QuickActionService quick)
    {
        _settings = settings;
        _ai = ai;
        _store = store;
        _quick = quick;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        await _store.LoadAsync();
        await _ai.InitializeAsync();
        IsAiUnavailable = !_ai.IsAvailable;
        AiStatusMessage = _ai.StatusMessage;
    }

    public void StartNewConversation()
    {
        _conversation = new Conversation();
        _conversationSaved = false;
        Messages.Clear();
        AddGreeting();
    }

    public void LoadConversation(Conversation conversation)
    {
        _conversation = conversation;
        _conversationSaved = true;
        Messages.Clear();
        foreach (var message in conversation.Messages)
            Messages.Add(message);
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        var input = InputText.Trim();
        if (input.Length == 0) return;
        InputText = string.Empty;

        if (!_conversationSaved)
        {
            _conversation = _store.NewConversation(input);
            _conversationSaved = true;
        }

        var userMessage = new ChatMessage { Role = "user", Text = input, At = DateTime.Now };
        Messages.Add(userMessage);
        IsBusy = true;
        try
        {
            var reply = await TryHandleLocalCommandAsync(input)
                ?? await _ai.ChatAsync(BuildSystemPrompt(), _conversation.Messages, input)
                ?? (_ai.IsAvailable
                    ? "I had trouble generating that reply just now — mind trying again?"
                    : LocalFallbackReply());

            var assistantMessage = new ChatMessage { Role = "assistant", Text = reply, At = DateTime.Now };
            _store.AddMessage(_conversation, userMessage);
            _store.AddMessage(_conversation, assistantMessage);
            Messages.Add(assistantMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSend() => !IsBusy && !string.IsNullOrWhiteSpace(InputText);

    [RelayCommand]
    private void NewConversation() => StartNewConversation();

    private void AddGreeting()
    {
        var name = _settings.AssistantName;
        Messages.Add(new ChatMessage
        {
            Role = "assistant",
            Text = $"Hi, I'm {name}! I run entirely on this PC — no cloud, no accounts, no tracking.\n\nAsk me anything, or type \"help\" to see what I can do right on this machine.",
            At = DateTime.Now,
        });
    }

    private string BuildSystemPrompt()
    {
        var name = _settings.AssistantName;
        var prompt = new StringBuilder(
            $"You are {name}, a warm, capable, slightly playful on-device Windows assistant created by St. John Pictures. " +
            "Answer concisely and helpfully.");

        if (_settings.RememberAboutMe)
        {
            var facts = _settings.GetUserFacts().Facts
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToList();
            if (facts.Count > 0)
            {
                prompt.Append(" Things you remember about the user: ");
                prompt.Append(string.Join("; ", facts));
                prompt.Append('.');
            }
        }

        return prompt.ToString();
    }

    /// <summary>
    /// Handles commands locally, without AI: greetings, help, time/date,
    /// "open …", "search …", "open folder …", and "my pc".
    /// Returns null when the input is not a local command.
    /// </summary>
    private async Task<string?> TryHandleLocalCommandAsync(string input)
    {
        var trimmed = input.Trim();
        var lower = trimmed.ToLowerInvariant();
        var name = _settings.AssistantName;

        if (lower is "hello" or "hi" or "hey" or "howdy"
            || lower.StartsWith("hello ", StringComparison.Ordinal)
            || lower.StartsWith("hi ", StringComparison.Ordinal)
            || lower.StartsWith("hey ", StringComparison.Ordinal))
            return $"Hi there! I'm {name}, running right here on your PC. What can I do for you?";

        if (lower is "help" or "?")
            return HelpText(name);

        if (lower.StartsWith("open folder ", StringComparison.Ordinal))
            return await _quick.OpenFolderAsync(trimmed.Substring("open folder ".Length));

        if (lower.StartsWith("open ", StringComparison.Ordinal))
            return await _quick.OpenAppAsync(trimmed.Substring("open ".Length));

        if (lower.StartsWith("search ", StringComparison.Ordinal))
            return await _quick.WebSearchAsync(trimmed.Substring("search ".Length));

        if (lower is "my pc" or "machine info" or "computer info" or "system info"
            || lower.StartsWith("my pc ", StringComparison.Ordinal))
            return await _quick.MachineInfoAsync();

        if (lower is "time" or "what time is it")
            return $"It's {DateTime.Now:t}.";

        if (lower is "date" or "what date is it" or "today's date")
            return $"Today is {DateTime.Now:D}.";

        return null;
    }

    private static string HelpText(string name) =>
        $"Here's what I can do on this PC:\n\n" +
        "• \"open calculator\" — open apps (calculator, notepad, paint, settings, store, terminal, browser, files)\n" +
        "• \"search <words>\" — search the web\n" +
        "• \"open folder <path>\" — open a folder\n" +
        "• \"my pc\" — show this PC's info\n" +
        "• \"time\" / \"date\" — the current time and date\n\n" +
        $"And of course, you can just chat with me — I'm {name}.";

    private static string LocalFallbackReply() =>
        "On-device AI isn't available on this PC, so I'm answering in local mode. " +
        "I can still open apps (\"open calculator\"), search the web (\"search <words>\"), " +
        "show machine info (\"my pc\"), and open folders (\"open folder <path>\"). " +
        "Type \"help\" for the full list.";
}
