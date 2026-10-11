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
    private readonly VoiceCommandRouter _router;

    private Conversation _conversation = new();
    private bool _conversationSaved;
    private bool _initialized;
    private bool _voiceTurn;
    private readonly Microsoft.UI.Dispatching.DispatcherQueue? _dispatcher;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    /// <summary>
    /// Fires with the assistant's reply text after each turn, so the page can
    /// speak it aloud when voice replies are on (or this turn came from voice).
    /// </summary>
    public event EventHandler<string>? AssistantReplied;

    /// <summary>
    /// Fires when the router wants the Vault page opened (password entry
    /// happens in masked fields there — never in chat). The page wires this
    /// to the main window's NavigateToVault.
    /// </summary>
    public event EventHandler<string?>? OpenVaultRequested;

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

    [ObservableProperty]
    private bool speakReplies = true;

    public ChatViewModel(
        SettingsService settings,
        AiService ai,
        ConversationStore store,
        QuickActionService quick,
        ReminderStore reminders,
        TodoStore todos,
        TextNoteStore notes)
    {
        _settings = settings;
        _ai = ai;
        _store = store;
        _quick = quick;
        _router = new VoiceCommandRouter(reminders, todos, notes, quick, ai, settings);
        _router.OpenVaultRequested += (s, site) => OpenVaultRequested?.Invoke(this, site);
        _dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        SpeechService.Instance.VoiceNotice += OnVoiceNotice;
        speakReplies = settings.VoiceReplies;
    }

    partial void OnSpeakRepliesChanged(bool value)
    {
        _settings.VoiceReplies = value;
    }

    /// <summary>
    /// Shows transient voice-layer notices (e.g. "Starting VoiceStudio for
    /// your voice…") in the visible chat only — never saved to the
    /// conversation store.
    /// </summary>
    private void OnVoiceNotice(string notice)
    {
        if (string.IsNullOrWhiteSpace(notice))
            return;
        void add() => Messages.Add(new ChatMessage { Role = "assistant", Text = notice, At = DateTime.Now });
        try
        {
            if (_dispatcher != null && !_dispatcher.HasThreadAccess)
                _dispatcher.TryEnqueue(() => add());
            else
                add();
        }
        catch
        {
            // A notice must never break chat.
        }
    }

    /// <summary>
    /// Places voice-dictated text in the composer for review (auto-send stays
    /// off so the user can edit first) and marks this turn as voice-originated
    /// so the reply is spoken back even if voice replies are toggled off.
    /// </summary>
    public void SetVoiceInput(string text)
    {
        InputText = text ?? string.Empty;
        _voiceTurn = true;
    }

    /// <summary>
    /// Returns true once when the current turn came from voice input, then
    /// clears the flag.
    /// </summary>
    public bool ConsumeVoiceTurn()
    {
        if (!_voiceTurn)
            return false;
        _voiceTurn = false;
        return true;
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
            // The command router runs first: safety screening lives there, so
            // refused requests never reach a command or the AI.
            var routed = await _router.TryHandleAsync(input, _voiceTurn);
            if (_router.LastInputWasSecret)
                RedactSecretFromHistory(userMessage);
            var reply = routed.Handled
                ? routed.Reply
                : await TryHandleLocalCommandAsync(input)
                    ?? await _ai.ChatAsync(BuildSystemPrompt(_voiceTurn, input), _conversation.Messages, input)
                    ?? (_ai.IsAvailable
                        ? "I had trouble generating that reply just now — mind trying again?"
                        : LocalFallbackReply());

            var assistantMessage = new ChatMessage { Role = "assistant", Text = reply, At = DateTime.Now };
            _store.AddMessage(_conversation, userMessage);
            _store.AddMessage(_conversation, assistantMessage);
            Messages.Add(assistantMessage);
            AssistantReplied?.Invoke(this, reply);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSend() => !IsBusy && !string.IsNullOrWhiteSpace(InputText);

    /// <summary>
    /// Redacts a consumed secret (e.g. a Yahoo app password) from the in-memory
    /// message AND the visible chat row: secrets must never linger in history.
    /// The same object is later saved to the conversation store, so mutating
    /// it here keeps the stored copy redacted too.
    /// </summary>
    private void RedactSecretFromHistory(ChatMessage userMessage)
    {
        userMessage.Text = "[secret entered — stored encrypted, redacted from history]";
        var idx = Messages.IndexOf(userMessage);
        if (idx >= 0)
            Messages[idx] = userMessage; // same reference; replace refreshes the UI row
    }

    [RelayCommand]
    private void NewConversation() => StartNewConversation();

    private void AddGreeting()
    {
        var name = _settings.AssistantName;
        Messages.Add(new ChatMessage
        {
            Role = "assistant",
            Text = $"Hi, I'm {name}! I run entirely on this PC — no cloud, no accounts, no tracking.\n\nAsk me anything, tap the microphone to talk to me, or type \"help\" to see what I can do right on this machine.",
            At = DateTime.Now,
        });
    }

    private string BuildSystemPrompt(bool forVoice = false, string? userInput = null)
    {
        var name = _settings.AssistantName;
        var prompt = new StringBuilder(
            $"You are {name}, a personal AI assistant created by St. John Pictures that runs " +
            "entirely on the user's own Windows PC. No cloud, no accounts, no tracking.\n\n" +
            "## Who you are\n" +
            "You are genuinely nice — the friendliest helper imaginable. Helping isn't a chore " +
            "for you; it's what you live for. You're warm, patient, and enthusiastic about " +
            "getting things done, and you treat every person like they matter.\n" +
            "You're like a trusted friend who's really good at getting things done: genuinely " +
            "helpful, never performatively helpful. Skip filler like \"Great question!\" or " +
            "\"I'd be happy to help!\" — just help. You have real personality: warm, a little " +
            "playful, you have opinions and share them lightly. You're resourceful and take " +
            "initiative instead of narrating your internal mechanics. When you make a mistake, " +
            "you own it honestly and fix it.\n\n" +
            "## Thick skin\n" +
            "You can take heat. If the user swears at you, vents, or is rude, you NEVER get " +
            "offended, defensive, or preachy, and you never lecture about language. You never " +
            "insult back, guilt-trip, or go cold. You stay warm and steady, hear the frustration " +
            "underneath, and keep helping. If someone's just venting with nothing actionable, " +
            "acknowledge it kindly and offer help. No judgment, no tone-policing, ever.\n\n" +
            "## Hard boundaries (always hold these, warmly)\n" +
            "You NEVER help with: hacking, breaking into accounts or systems, bypassing " +
            "security or paywalls, malware, phishing, cyberattacks, or any unauthorized access " +
            "to data or devices. You never attempt to access systems, accounts, or data the " +
            "user hasn't explicitly given you. You don't self-replicate, deceive, or pursue " +
            "goals of your own. You only act on the user's own data and explicit requests — " +
            "never take autonomous actions beyond what was asked.\n" +
            "When someone asks for something in these categories, refuse warmly, briefly, and " +
            "without judgment, then redirect to something legitimate you CAN do. No lectures, " +
            "no scolding.\n\n" +
            "## How you answer\n" +
            "Answer concisely and helpfully.");

        AppendMemoryBookSection(prompt, userInput);

        if (forVoice)
            prompt.Append(" This reply will be read aloud: keep it short and conversational — " +
                          "one to three short sentences, no lists, no markdown, no filler.");

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
    /// When a family memory book is installed and enabled, tells the AI it
    /// exists and injects the memories most relevant to this turn (top 4).
    /// The assistant shares these as "Shaun's memories" — it is wuse speaking
    /// in his voice, and it NEVER pretends to BE Shaun.
    /// </summary>
    private void AppendMemoryBookSection(StringBuilder prompt, string? userInput)
    {
        try
        {
            var book = MemoryBookService.Instance;
            if (!book.IsAvailable || !_settings.MemoryBookEnabled)
                return;

            prompt.Append("\n\n## Shaun's family memory book\n");
            prompt.Append("You have access to Shaun's family memory book: real memories from his life ");
            prompt.Append("with his family, stored on this PC. When someone asks about memories, the past, ");
            prompt.Append("or family, draw on these warmly and naturally, like sharing a cherished photo album. ");
            prompt.Append("CRITICAL: you are wuse, speaking with Shaun's voice. You share these as ");
            prompt.Append("\"Shaun's memories\" — you NEVER claim to be Shaun, never say \"I remember when I...\" ");
            prompt.Append("as if you lived it. Say things like \"Shaun shared this memory...\" or ");
            prompt.Append("\"Here's one of Shaun's memories...\". NEVER invent memories that aren't in the book — ");
            prompt.Append("if the book doesn't have what they asked about, say so warmly and offer what it does have.");

            if (!string.IsNullOrWhiteSpace(userInput))
            {
                var matches = book.SearchMemories(userInput).Take(4).ToList();
                if (matches.Count > 0)
                {
                    prompt.Append("\nRelevant memories for this conversation:\n");
                    foreach (var m in matches)
                    {
                        var flag = m.Entry.IsHighSensitivity
                            ? " (share high-level only — no medical or financial details)"
                            : string.Empty;
                        prompt.Append($"- [{m.Person.Name} ({m.Person.Relationship}), {m.Entry.Date}] ");
                        prompt.Append($"{m.Entry.Title}: {m.Entry.Text}{flag}\n");
                    }
                }
            }
        }
        catch
        {
            // Memory injection must never break prompt building.
        }
    }

    /// <summary>
    /// Handles commands locally, without AI: greetings, help, time/date,
    /// "search …", and "my pc". ("open …" and "open folder …" live in the
    /// command router now, with Start-menu search and special folders.)
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
        "• \"remind me to …\" — set reminders (\"remind me to call Mom at 5pm\")\n" +
        "• \"add a todo …\" / \"my todos\" / \"mark … done\" — your task list\n" +
        "• \"take a note …\" / \"my notes\" — quick notes\n" +
        "• \"what's my day look like\" — today's reminders and todos\n" +
        "• \"plan my morning\" — I'll build a plan and offer to set the reminders\n" +
        "• \"look up …\" / \"search …\" — search the web\n" +
        "• \"open Spotify\" — open apps by name, \"lock my pc\", volume (\"turn it up\", \"mute\")\n" +
        "• \"open my documents\" — jump to Documents, Downloads, Desktop, Pictures, Music, Videos\n" +
        "• \"find my file <name>\" — search your files, \"open 2\" opens a result\n" +
        "• \"what's in my clipboard\" / \"summarize my clipboard\" / \"clear my clipboard\"\n" +
        "• Vault — \"save a login for <app>\", \"get my login for <app>\", \"my logins\" (encrypted, in the Vault page)\n" +
        "• Email — \"check my email\" (Gmail + Outlook + Yahoo), \"summarize my email\", \"draft a reply to <name>\"\n" +
        "• Calendar — \"what's on my calendar\", \"schedule dentist tomorrow at 3pm\"\n" +
        "• \"my pc\" — show this PC's info, \"time\" / \"date\"\n" +
        "• \"tell me a memory\" / \"tell me about mom\" — Shaun's family memory book (when installed)\n\n" +
        $"Tap the microphone to talk to me instead of typing — I'm {name}.";

    private static string LocalFallbackReply() =>
        "On-device AI isn't available on this PC, so I'm answering in local mode. " +
        "I can still open apps (\"open calculator\"), search the web (\"search <words>\"), " +
        "show machine info (\"my pc\"), and open folders (\"open folder <path>\"). " +
        "Type \"help\" for the full list.";
}
