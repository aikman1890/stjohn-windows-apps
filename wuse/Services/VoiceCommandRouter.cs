using System.Text;
using System.Text.RegularExpressions;
using Wuse.Models;

namespace Wuse.Services;

/// <summary>
/// One block of a proposed multi-step plan ("plan my morning").
/// </summary>
public sealed class PlanBlock
{
    public string Text { get; set; } = string.Empty;
    public DateTime At { get; set; }
}

/// <summary>
/// A proposed outward-facing action awaiting the user's explicit "yes".
/// Destructive or outward-facing actions (send email, create calendar event,
/// delete a login) NEVER run without this confirmation.
/// </summary>
public sealed class PendingConfirmation
{
    public string Prompt { get; set; } = string.Empty;
    public Func<Task<string>> RunAsync { get; set; } = () => Task.FromResult(string.Empty);
}

/// <summary>
/// Routes user requests (typed or spoken) to wuse's built-in command set.
/// Safety screening runs FIRST — refused requests never reach a command.
/// Returns Handled=false when the input isn't a command (caller falls through
/// to the on-device AI).
/// </summary>
public sealed class VoiceCommandRouter
{
    private readonly ReminderStore _reminders;
    private readonly TodoStore _todos;
    private readonly TextNoteStore _notes;
    private readonly QuickActionService _quick;
    private readonly AiService _ai;
    private readonly ReminderParser _parser = new();
    private readonly SettingsService _settings;

    // Wave 2 services.
    private readonly PcControlService _pc = new();
    private readonly FileFinderService _files = new();
    private readonly ClipboardService _clipboard = new();
    private readonly VaultService _vault = new();
    private readonly GoogleAuthService _googleAuth = new();
    private readonly MicrosoftAuthService _msAuth = new();
    private readonly GmailClient _gmail;
    private readonly GoogleCalendarClient _gcal;
    private readonly OutlookMailClient _outlookMail;
    private readonly OutlookCalendarClient _outlookCal;
    private readonly YahooMailClient _yahoo;
    private readonly EmailAggregator _email;

    // Pending multi-turn state.
    private List<PlanBlock>? _pendingPlan;
    private PendingConfirmation? _pendingConfirm;
    private string? _pendingVaultSite;
    private bool _pendingYahooEmail;
    private bool _pendingYahooPassword;
    private string _pendingYahooAddress = string.Empty;
    private List<FileHit>? _pendingFiles;

    /// <summary>
    /// True when the just-handled turn consumed a secret (Yahoo app password).
    /// The chat layer uses this to redact the message from history.
    /// </summary>
    public bool LastInputWasSecret { get; private set; }

    /// <summary>
    /// Fires when chat should open the Vault page (password entry happens in
    /// masked fields there — never in chat). Parameter is an optional site
    /// name to prefill.
    /// </summary>
    public event EventHandler<string?>? OpenVaultRequested;

    private static readonly string[] StopWords =
    {
        "the", "a", "an", "my", "to", "for", "please", "that", "this", "it",
        "reminder", "reminders", "todo", "todos", "task", "tasks", "note", "notes",
    };

    public VoiceCommandRouter(
        ReminderStore reminders,
        TodoStore todos,
        TextNoteStore notes,
        QuickActionService quick,
        AiService ai,
        SettingsService settings)
    {
        _reminders = reminders;
        _todos = todos;
        _notes = notes;
        _quick = quick;
        _ai = ai;
        _settings = settings;
        _gmail = new GmailClient(_googleAuth);
        _gcal = new GoogleCalendarClient(_googleAuth);
        _outlookMail = new OutlookMailClient(_msAuth);
        _outlookCal = new OutlookCalendarClient(_msAuth);
        _yahoo = new YahooMailClient(_vault);
        _email = new EmailAggregator(_gmail, _outlookMail, _yahoo);
    }

    /// <summary>
    /// Family-mode gate. Called by the chat view model INSTEAD of the full
    /// command router when SettingsService.FamilyMode is on. Nothing
    /// executes here: refused requests get the safety refusal, private
    /// topics get one boundary reply, and everything else returns
    /// unhandled so the view model answers with on-device AI chat only.
    /// Any pending Shaun-side flow (vault setup, confirmations) is
    /// dropped so a family turn can never complete it.
    /// </summary>
    public (bool Handled, string Reply) TryHandleFamilyAsync(string input)
    {
        var text = (input ?? string.Empty).Trim();
        LastInputWasSecret = false;
        if (text.Length == 0)
            return (false, string.Empty);

        if (SafetyGuard.IsRefused(text, out var category))
        {
            ClearPending();
            return (true, SafetyGuard.WarmRefusal(category));
        }

        ClearPending();
        var lower = text.ToLowerInvariant();
        if (Regex.IsMatch(lower,
                @"\b(vault|password|login|logins|email|mail|gmail|outlook|yahoo|calendar|reminder|appointment|file|files|document|folder|memory|memories|conversation|bank|account|debt|court|lawsuit|health|doctor|wife|divorce|money|payment|tax)\b"))
        {
            return (true,
                "Family mode keeps Shaun's private stuff private — I can't open his email, vault, files, " +
                "calendar, or memories, and I don't share his personal details. I can still chat, talk about " +
                "his books and videos, and help with general questions. Ask me about those.");
        }

        return (false, string.Empty);
    }

    public async Task<(bool Handled, string Reply)> TryHandleAsync(string input, bool fromVoice = false)
    {
        var text = (input ?? string.Empty).Trim();
        LastInputWasSecret = false;
        if (text.Length == 0)
            return (false, string.Empty);

        // 1. Safety first — refused requests never reach a command.
        if (SafetyGuard.IsRefused(text, out var category))
        {
            ClearPending();
            return (true, SafetyGuard.WarmRefusal(category));
        }

        var lower = text.ToLowerInvariant();

        // 2. Pending explicit confirmation ("yes" / "no" / something else).
        if (_pendingConfirm != null)
        {
            var pending = _pendingConfirm;
            if (IsAffirmative(lower))
            {
                _pendingConfirm = null;
                return (true, await SafeRunAsync(pending.RunAsync));
            }
            if (IsNegative(lower))
            {
                _pendingConfirm = null;
                return (true, "OK — I won't do that.");
            }
            // A fresh command, not an answer: drop the pending action and
            // handle this turn normally.
            _pendingConfirm = null;
        }

        // 3. Bare confirmation applies only to a pending plan offer.
        if (_pendingPlan != null)
        {
            if (IsAffirmative(lower))
            {
                var blocks = _pendingPlan;
                _pendingPlan = null;
                return (true, await CreatePlanRemindersAsync(blocks));
            }
            _pendingPlan = null; // anything else moves on; the offer lapses.
        }

        // 4. Pending vault username (chat step of "save a login for X").
        if (_pendingVaultSite != null)
        {
            var site = _pendingVaultSite;
            _pendingVaultSite = null;
            var username = text; // shown in chat is fine; password is NOT collected here
            var voiceNudge = fromVoice
                ? " Since we're talking out loud, please type the password rather than saying it."
                : string.Empty;
            OpenVaultRequested?.Invoke(this, site);
            return (true,
                $"Got it — username \"{username}\" noted for {site}. I've opened the Vault " +
                $"for you: type the password into the masked password field there and hit Save.{voiceNudge} " +
                "I never ask for passwords in chat, spoken or typed.");
        }

        // 5. Pending Yahoo setup: email, then app password (secret).
        if (_pendingYahooEmail)
        {
            _pendingYahooEmail = false;
            _pendingYahooAddress = text;
            return (true,
                "Thanks. Now paste your Yahoo app password (the one from login.yahoo.com → " +
                "Account Security → \"Generate app password\"). It goes straight into the " +
                "encrypted vault — I won't show it anywhere.");
        }
        if (_pendingYahooPassword)
        {
            _pendingYahooPassword = false;
            LastInputWasSecret = true; // chat layer redacts this turn from history
            _yahoo.SaveAppPassword(_pendingYahooAddress, text);
            _pendingYahooAddress = string.Empty;
            return (true,
                "Saved — your Yahoo app password is encrypted in the vault. " +
                "Say \"check my yahoo\" any time.");
        }

        // 6. "open <number>" applies to pending file-search results.
        if (_pendingFiles != null)
        {
            var openNum = Regex.Match(lower, @"^open (?:file )?(\d+)$");
            if (openNum.Success && int.TryParse(openNum.Groups[1].Value, out int n)
                && n >= 1 && n <= _pendingFiles.Count)
            {
                var hit = _pendingFiles[n - 1];
                _pendingFiles = null;
                return (true, await _files.OpenAsync(hit));
            }
            _pendingFiles = null; // anything else moves on; the list lapses.
        }

        // 7. Multi-step: "plan my morning" / "plan my day" / "plan tomorrow".
        if (Regex.IsMatch(lower, @"\bplan my (morning|day)\b") || lower.Contains("plan tomorrow"))
            return (true, await PlanDayAsync(lower.Contains("plan tomorrow")));

        // 8. Reminders.
        if (Regex.IsMatch(lower, @"^(please\s+)?(could you|can you\s+)?remind me\b"))
            return (true, await AddReminderAsync(text));
        if (Regex.IsMatch(lower, @"\b(my reminders|what.*reminders|show.*reminders|list.*reminders|reminders list)\b"))
            return (true, await ListRemindersAsync());
        var cancelReminder = Regex.Match(lower, @"\b(cancel|delete|remove)\s+(the\s+)?reminder\s+(.+)");
        if (cancelReminder.Success)
            return (true, await CancelReminderAsync(cancelReminder.Groups[3].Value));

        // 9. Todos.
        var addTodo = Regex.Match(text, @"^(please\s+)?(add\s+)?(a\s+)?(todo|task)\b\s*[:\-]?\s*(.+)$", RegexOptions.IgnoreCase);
        if (addTodo.Success && addTodo.Groups[5].Value.Trim().Length > 0)
            return (true, await AddTodoAsync(addTodo.Groups[5].Value.Trim()));
        if (Regex.IsMatch(lower, @"\b(my todos|my tasks|todo list|task list|what.*todos|what.*tasks|show.*todos|list.*todos)\b"))
            return (true, await ListTodosAsync());
        var completeTodo = Regex.Match(lower, @"\b(complete|mark|finish)\b.{0,10}\b(done|complete|finished|off)\b\s*(.+)|^mark\s+(.+?)\s+done$|^complete\s+(.+)$|^finish\s+(.+)$");
        if (completeTodo.Success)
        {
            var target = FirstNonEmpty(completeTodo.Groups[3].Value, completeTodo.Groups[4].Value,
                completeTodo.Groups[5].Value, completeTodo.Groups[6].Value);
            if (target.Length > 0)
                return (true, await CompleteTodoAsync(target));
        }
        if (lower.Contains("clear completed"))
            return (true, await ClearCompletedTodosAsync());

        // 10. Notes.
        var takeNote = Regex.Match(text, @"^(please\s+)?(take a note|note that|note)\b\s*[:\-]?\s*(.+)$", RegexOptions.IgnoreCase);
        if (takeNote.Success && takeNote.Groups[3].Value.Trim().Length > 0)
            return (true, await AddNoteAsync(takeNote.Groups[3].Value.Trim()));
        if (Regex.IsMatch(lower, @"\b(my notes|read.*notes|show.*notes|what.*notes)\b"))
            return (true, await ListNotesAsync());

        // 11. Day questions.
        if (Regex.IsMatch(lower, @"what'?s my day|what do i have today|what'?s on today|^my day$|how does (my|today) look"))
            return (true, await DaySummaryAsync());

        // 12. PC control: open/launch apps (extended: Start-menu search).
        // "open folder <path>" is checked first so it isn't eaten by "open".
        var openFolder = Regex.Match(text, @"^open folder\s+(.+)$", RegexOptions.IgnoreCase);
        if (openFolder.Success)
            return (true, await _quick.OpenFolderAsync(openFolder.Groups[1].Value.Trim()));
        if (Regex.IsMatch(lower, @"^open (the )?vault$"))
        {
            OpenVaultRequested?.Invoke(this, null);
            return (true, "Opening your vault — logins are encrypted in the Windows Credential Locker.");
        }
        var openApp = Regex.Match(text, @"^(please\s+)?(open|launch|start)\s+(.+)$", RegexOptions.IgnoreCase);
        if (openApp.Success)
        {
            var target = openApp.Groups[3].Value.Trim();
            // "open my documents/downloads/…" are folders, not apps.
            if (Regex.IsMatch(target, @"^(my\s+)?(documents|downloads|desktop|pictures|music|videos)$", RegexOptions.IgnoreCase))
                return (true, await _pc.OpenSpecialFolderAsync(target));
            return (true, await _pc.OpenAppAsync(target));
        }

        // 13. Volume.
        if (Regex.IsMatch(lower, @"\b(turn (it|the volume) (up|down)|volume (up|down)|turn up|turn down)\b"))
        {
            var dir = lower.Contains("down") ? "down" : "up";
            return (true, _pc.AdjustVolume(dir));
        }
        if (Regex.IsMatch(lower, @"\b(mute|unmute)\b"))
            return (true, _pc.AdjustVolume(lower.Contains("unmute") ? "unmute" : "mute"));

        // 14. Lock PC.
        if (Regex.IsMatch(lower, @"\block (my |the )?(pc|computer)\b"))
            return (true, _pc.LockPc());

        // 15. File finding. (Before the generic "find" web lookup below.)
        var findFile = Regex.Match(text, @"^(please\s+)?find (my )?file\s+(.+)$", RegexOptions.IgnoreCase);
        if (findFile.Success)
            return (true, await FindFileAsync(findFile.Groups[3].Value.Trim()));

        // 16. Clipboard.
        if (Regex.IsMatch(lower, @"^(what'?s|what is|show)(.*)in my clipboard$") || lower == "show my clipboard")
            return (true, ShowClipboard());
        if (lower.Contains("summarize my clipboard"))
            return (true, await SummarizeClipboardAsync());
        if (Regex.IsMatch(lower, @"\b(clear|empty|wipe)\b.{0,10}\bclipboard\b"))
        {
            _clipboard.Clear();
            return (true, "Clipboard cleared.");
        }

        // 17. Vault.
        var saveLogin = Regex.Match(text, @"^(please\s+)?save (a )?login for\s+(.+)$", RegexOptions.IgnoreCase);
        if (saveLogin.Success)
        {
            var site = saveLogin.Groups[3].Value.Trim();
            if (site.Length == 0)
                return (true, "Which app or site is the login for?");
            _pendingVaultSite = site;
            var voiceNudge = fromVoice
                ? " (Since we're talking out loud, I'll have you type the secrets — never say a password.)"
                : string.Empty;
            return (true, $"I'll save a login for {site}.{voiceNudge} What's the username?");
        }
        var getLogin = Regex.Match(text, @"^(please\s+)?(get|show|give me)( my)? login for\s+(.+)$", RegexOptions.IgnoreCase);
        if (getLogin.Success)
            return (true, await CopyLoginPasswordAsync(getLogin.Groups[4].Value.Trim()));
        if (Regex.IsMatch(lower, @"\b(my logins|list (my )?logins|show (my )?logins|vault)\b"))
            return (true, ListLogins());
        var deleteLogin = Regex.Match(text, @"^(please\s+)?delete (my )?login for\s+(.+)$", RegexOptions.IgnoreCase);
        if (deleteLogin.Success)
            return (true, AskDeleteLogin(deleteLogin.Groups[3].Value.Trim()));

        // 18. Email: connect flows.
        if (Regex.IsMatch(lower, @"\bconnect (my )?(gmail|google)\b"))
            return (true, await ConnectGoogleAsync());
        if (Regex.IsMatch(lower, @"\bconnect (my )?(outlook|hotmail|microsoft|office ?365)\b"))
            return (true, await ConnectMicrosoftAsync());
        if (Regex.IsMatch(lower, @"\bconnect (my )?yahoo\b"))
        {
            _pendingYahooEmail = true;
            return (true, "To connect Yahoo, I'll need your Yahoo email address first — what's the address?");
        }
        if (Regex.IsMatch(lower, @"\bdisconnect yahoo\b"))
        {
            _yahoo.Disconnect();
            return (true, "Yahoo disconnected — I threw away the saved app password.");
        }

        // 19. Email: check.
        if (Regex.IsMatch(lower, @"\bcheck my (gmail|google mail)\b"))
            return (true, await CheckOneAsync("Gmail", () => _gmail.GetUnreadAsync(10)));
        if (Regex.IsMatch(lower, @"\bcheck my (outlook|hotmail|microsoft mail|office ?365 mail)\b"))
            return (true, await CheckOneAsync("Outlook", () => _outlookMail.GetUnreadAsync(10)));
        if (Regex.IsMatch(lower, @"\bcheck my yahoo\b"))
            return (true, await CheckOneAsync("Yahoo", () => _yahoo.GetUnreadAsync(10)));
        if (Regex.IsMatch(lower, @"\bcheck my email\b") || lower == "check email")
            return (true, (await _email.CheckAllAsync()).Reply);

        // 20. Email: summarize + draft reply.
        if (Regex.IsMatch(lower, @"\bsummarize my email"))
            return (true, await SummarizeEmailAsync());
        var draftReply = Regex.Match(text, @"^(please\s+)?draft (a )?reply to\s+(.+)$", RegexOptions.IgnoreCase);
        if (draftReply.Success)
            return (true, await DraftReplyAsync(draftReply.Groups[3].Value.Trim(), fromVoice));

        // 21. Calendar.
        if (Regex.IsMatch(lower, @"\b(what'?s on my calendar|my calendar( today)?|my schedule( today)?|what do i have (on |scheduled )?today)\b"))
            return (true, await CalendarTodayAsync());
        var schedule = Regex.Match(text, @"^(please\s+)?schedule\s+(.+)$", RegexOptions.IgnoreCase);
        if (schedule.Success)
            return (true, await ProposeEventAsync(schedule.Groups[2].Value.Trim()));

        // 22. Family memory book ("tell me a memory", "tell me about mom",
        // "do you remember when ..."). Conversational only — no page.
        var memoryReply = TryHandleMemoryBook(text, lower);
        if (memoryReply != null)
            return (true, memoryReply);

        // 23. Web lookups ("look up ...", "find ..." — "search ..." lives in the
        // existing local-command handler).
        var lookUp = Regex.Match(text, @"^(please\s+)?(look up|find)\s+(.+)$", RegexOptions.IgnoreCase);
        if (lookUp.Success)
            return (true, await _quick.WebSearchAsync(lookUp.Groups[3].Value.Trim()));

        return (false, string.Empty);
    }

    private void ClearPending()
    {
        _pendingPlan = null;
        _pendingConfirm = null;
        _pendingVaultSite = null;
        _pendingYahooEmail = false;
        _pendingYahooPassword = false;
        _pendingYahooAddress = string.Empty;
        _pendingFiles = null;
    }

    private static bool IsNegative(string lower) =>
        lower is "no" or "nope" or "nah" or "cancel" or "never mind" or "nevermind"
        || lower.StartsWith("no ", StringComparison.Ordinal)
        || lower.StartsWith("no,", StringComparison.Ordinal)
        || lower.StartsWith("don't ", StringComparison.Ordinal)
        || lower.StartsWith("do not ", StringComparison.Ordinal);

    private static bool IsAffirmative(string lower) =>
        lower is "yes" or "yeah" or "yep" or "sure" or "ok" or "okay" or "do it" or "please do"
        || lower.StartsWith("yes ", StringComparison.Ordinal)
        || lower.StartsWith("yes,", StringComparison.Ordinal)
        || lower.StartsWith("sure ", StringComparison.Ordinal);

    private static string FirstNonEmpty(params string[] values)
    {
        foreach (var v in values)
            if (!string.IsNullOrWhiteSpace(v))
                return v.Trim();
        return string.Empty;
    }

    private static async Task<string> SafeRunAsync(Func<Task<string>> run)
    {
        try
        {
            return await run();
        }
        catch (Exception ex)
        {
            return $"Something went wrong doing that: {ex.Message}";
        }
    }

    // ---------- Reminders ----------

    private async Task<string> AddReminderAsync(string text)
    {
        try
        {
            var (due, reminderText) = _parser.Parse(text);
            await _reminders.AddAsync(new ReminderItem { Text = reminderText, DueAt = due });
            return $"Got it — I'll remind you to {reminderText} at {due:h:mm tt} on {due:MMMM d}. Anything else I can do for you?";
        }
        catch (Exception)
        {
            return "I couldn't set that reminder just now — mind saying it once more?";
        }
    }

    private async Task<string> ListRemindersAsync()
    {
        try
        {
            var upcoming = await _reminders.GetUpcomingAsync();
            if (upcoming.Count == 0)
                return "You've got no reminders set. Just say \"remind me to …\" and I'll take care of it.";
            var sb = new StringBuilder($"You've got {upcoming.Count} reminder{(upcoming.Count == 1 ? "" : "s")} coming up: ");
            sb.Append(string.Join("; ", upcoming.Take(8).Select(r => $"{r.Text} at {r.DueAt:h:mm tt} on {r.DueAt:MMM d}")));
            sb.Append('.');
            return sb.ToString();
        }
        catch (Exception)
        {
            return "I couldn't pull up your reminders just now.";
        }
    }

    private async Task<string> CancelReminderAsync(string target)
    {
        try
        {
            var match = FindBestMatch(await _reminders.GetUpcomingAsync(), r => r.Text, target);
            if (match == null)
                return "I couldn't find a reminder matching that. Say \"my reminders\" to hear the full list.";
            await _reminders.DeleteAsync(match.Id);
            return $"Done — cancelled your reminder to {match.Text}.";
        }
        catch (Exception)
        {
            return "I couldn't cancel that reminder just now.";
        }
    }

    // ---------- Todos ----------

    private async Task<string> AddTodoAsync(string text)
    {
        try
        {
            await _todos.AddAsync(new TodoItem { Text = text });
            return $"Done — \"{text}\" is on your todo list.";
        }
        catch (Exception)
        {
            return "I couldn't add that todo just now — mind trying once more?";
        }
    }

    private async Task<string> ListTodosAsync()
    {
        try
        {
            var open = await _todos.GetOpenAsync();
            if (open.Count == 0)
                return "Your todo list is clear — nice! Say \"add a todo …\" any time something comes up.";
            var sb = new StringBuilder($"You've got {open.Count} open todo{(open.Count == 1 ? "" : "s")}: ");
            sb.Append(string.Join("; ", open.Take(8).Select((t, i) => $"{i + 1}. {t.Text}")));
            sb.Append('.');
            return sb.ToString();
        }
        catch (Exception)
        {
            return "I couldn't pull up your todos just now.";
        }
    }

    private async Task<string> CompleteTodoAsync(string target)
    {
        try
        {
            var open = await _todos.GetOpenAsync();
            var match = FindBestMatch(open, t => t.Text, target);
            if (match == null)
                return "I couldn't find an open todo matching that. Say \"my todos\" to hear the list.";
            await _todos.MarkDoneAsync(match.Id);
            return $"Nice work — marked off: {match.Text}.";
        }
        catch (Exception)
        {
            return "I couldn't update that todo just now.";
        }
    }

    private async Task<string> ClearCompletedTodosAsync()
    {
        try
        {
            int removed = await _todos.ClearCompletedAsync();
            return removed == 0
                ? "Nothing to clear — no completed todos hanging around."
                : $"Cleared {removed} completed todo{(removed == 1 ? "" : "s")}. Fresh slate!";
        }
        catch (Exception)
        {
            return "I couldn't clear those just now.";
        }
    }

    // ---------- Notes ----------

    private async Task<string> AddNoteAsync(string text)
    {
        try
        {
            await _notes.AddAsync(new TextNote { Text = text });
            return "Saved! I jotted that down for you.";
        }
        catch (Exception)
        {
            return "I couldn't save that note just now — mind trying once more?";
        }
    }

    private async Task<string> ListNotesAsync()
    {
        try
        {
            var notes = await _notes.GetAllAsync();
            if (notes.Count == 0)
                return "No notes yet. Say \"take a note …\" and I'll remember it for you.";
            var sb = new StringBuilder($"Here are your latest {Math.Min(5, notes.Count)} notes: ");
            sb.Append(string.Join(" … ", notes.Take(5).Select(n => n.Text)));
            sb.Append('.');
            return sb.ToString();
        }
        catch (Exception)
        {
            return "I couldn't pull up your notes just now.";
        }
    }

    // ---------- Day summary ----------

    private async Task<string> DaySummaryAsync()
    {
        try
        {
            var today = await _reminders.GetDueOnAsync(DateTime.Today);
            var todos = await _todos.GetOpenAsync();
            if (today.Count == 0 && todos.Count == 0)
                return "Nothing on the books today — a wide-open day! Want me to add something to it?";

            var sb = new StringBuilder("Here's your day: ");
            var parts = new List<string>();
            parts.AddRange(today.Select(r => $"{r.Text} at {r.DueAt:h:mm tt}"));
            if (todos.Count > 0)
                parts.Add($"{todos.Count} open todo{(todos.Count == 1 ? "" : "s")}: " +
                          string.Join(", ", todos.Take(5).Select(t => t.Text)));
            sb.Append(string.Join(". ", parts));
            sb.Append(". You've got this!");
            return sb.ToString();
        }
        catch (Exception)
        {
            return "I couldn't pull your day together just now.";
        }
    }

    // ---------- File finding ----------

    private async Task<string> FindFileAsync(string query)
    {
        if (query.Length < 2)
            return "What file should I look for? Say \"find my file <name>\".";
        try
        {
            var hits = await _files.FindAsync(query);
            if (hits.Count == 0)
                return $"I didn't find anything matching \"{query}\" in your Documents, Downloads, Desktop, or OneDrive.";
            _pendingFiles = hits.ToList();
            var sb = new StringBuilder($"Found {hits.Count}: ");
            for (int i = 0; i < hits.Count; i++)
            {
                var h = hits[i];
                sb.Append($"{i + 1}. {h.Name} ({h.Directory}, {FileFinderService.FormatSize(h.SizeBytes)}); ");
            }
            sb.Append("Say \"open <number>\" to open one.");
            return sb.ToString().TrimEnd(' ', ';');
        }
        catch (Exception)
        {
            return "The file search hiccupped — mind trying again?";
        }
    }

    // ---------- Clipboard ----------

    private string ShowClipboard()
    {
        var text = _clipboard.GetText();
        if (text.Length == 0)
            return "Your clipboard is empty right now.";
        var shown = text.Length > 500 ? text.Substring(0, 500) + "…" : text;
        return $"Here's what's in your clipboard:\n\n{shown}";
    }

    private async Task<string> SummarizeClipboardAsync()
    {
        var text = _clipboard.GetText();
        if (text.Length == 0)
            return "There's nothing in your clipboard to summarize.";
        if (!_ai.IsAvailable)
            return "On-device AI isn't available, so I can't summarize it — but I can show it: say \"what's in my clipboard\".";
        try
        {
            var summary = await _ai.ChatAsync(
                "Summarize the following clipboard text briefly and helpfully, in plain words.",
                Array.Empty<ChatMessage>(),
                text.Length > 4000 ? text.Substring(0, 4000) : text);
            return string.IsNullOrWhiteSpace(summary)
                ? "I couldn't summarize that just now."
                : "Here's the gist of your clipboard: " + summary.Trim();
        }
        catch
        {
            return "I couldn't summarize that just now.";
        }
    }

    // ---------- Vault ----------

    private string ListLogins()
    {
        var all = _vault.GetAll();
        if (all.Count == 0)
            return "Your vault is empty. Say \"save a login for <app>\" and I'll walk you through it — " +
                   "everything's encrypted in the Windows vault, and I never show passwords.";
        var sb = new StringBuilder($"You've got {all.Count} saved login{(all.Count == 1 ? "" : "s")}: ");
        sb.Append(string.Join("; ", all.Take(15).Select(e =>
            string.IsNullOrWhiteSpace(e.Username) ? e.Name : $"{e.Name} ({e.Username})")));
        sb.Append(". Say \"get my login for <app>\" and I'll copy the password to your clipboard.");
        return sb.ToString();
    }

    private async Task<string> CopyLoginPasswordAsync(string site)
    {
        var match = _vault.FindBestMatch(site);
        if (match == null)
            return $"I couldn't find a login for \"{site}\". Say \"my logins\" to see what's saved.";
        var retrieved = _vault.Retrieve(match.Name);
        if (retrieved == null)
            return $"I couldn't find a login for \"{site}\".";
        // Password goes ONLY to the clipboard — never printed, never spoken.
        await _clipboard.CopyTextAsync(retrieved.Value.Password, autoClearAfterSeconds: 30);
        return $"Password for {match.Name} copied to your clipboard — it clears itself in 30 seconds. " +
               "Paste it wherever you need it.";
    }

    private string AskDeleteLogin(string site)
    {
        var match = _vault.FindBestMatch(site);
        if (match == null)
            return $"I couldn't find a login for \"{site}\". Say \"my logins\" to see what's saved.";
        var name = match.Name;
        _pendingConfirm = new PendingConfirmation
        {
            Prompt = $"Delete the login for {name}?",
            RunAsync = async () =>
            {
                await Task.CompletedTask;
                return _vault.Delete(name)
                    ? $"Deleted your login for {name}."
                    : $"I couldn't delete the login for {name}.";
            },
        };
        return $"Delete your saved login for {name}? This can't be undone. Say \"yes\" to confirm.";
    }

    // ---------- Email ----------

    private async Task<string> ConnectGoogleAsync()
    {
        if (!AppCredentials.IsGoogleConfigured)
            return "Google isn't set up yet — a Google client ID and secret need to go into the app's " +
                   "AppCredentials.cs first (one-time setup; the comments there walk through it). " +
                   "Tell me when that's done and I'll open the sign-in page.";
        var (ok, error, _) = await _googleAuth.EnsureCredentialAsync();
        return ok
            ? "Gmail and Google Calendar are connected! Say \"check my email\" any time."
            : error + " (I'll open the Google sign-in page in your browser — approve it there, then try again.)";
    }

    private async Task<string> ConnectMicrosoftAsync()
    {
        if (!AppCredentials.IsMicrosoftConfigured)
            return MicrosoftAuthService.MicrosoftSetupMessage;
        var (ok, message) = await _msAuth.AddAccountAsync();
        return ok ? message : message;
    }

    private async Task<string> CheckOneAsync(
        string label, Func<Task<(bool Ok, string Error, IReadOnlyList<EmailMessage> Messages)>> fetch)
    {
        var (ok, error, messages) = await fetch();
        if (!ok)
            return error;
        if (messages.Count == 0)
            return $"No unread mail on {label} — all caught up!";
        var sb = new StringBuilder($"{messages.Count} unread on {label}: ");
        sb.Append(string.Join("; ", messages.Take(10).Select(m => $"{m.From}: {m.Subject}")));
        sb.Append('.');
        return sb.ToString();
    }

    private async Task<string> SummarizeEmailAsync()
    {
        var (_, reply) = await _email.CheckAllAsync(6);
        // Gather bodies for the newest few unread messages.
        var all = new List<EmailMessage>();
        var g = await _gmail.GetUnreadAsync(5);
        if (g.Ok) all.AddRange(g.Messages);
        var o = await _outlookMail.GetUnreadAsync(5);
        if (o.Ok) all.AddRange(o.Messages);
        var y = await _yahoo.GetUnreadAsync(5);
        if (y.Ok) all.AddRange(y.Messages);

        if (all.Count == 0)
            return reply; // covers the not-connected case too

        if (!_ai.IsAvailable)
            return reply + "\n\n(On-device AI isn't available, so I can't write a summary — but the list above is your unread mail.)";

        var sb = new StringBuilder();
        foreach (var m in all.OrderByDescending(m => m.Received).Take(5))
        {
            var body = await _email.GetBodyAsync(m, 1500);
            sb.AppendLine($"--- [{m.Provider}] From: {m.From} | Subject: {m.Subject}");
            sb.AppendLine(body);
        }
        try
        {
            var summary = await _ai.ChatAsync(
                "Summarize these recent emails for the user: one short line per email " +
                "(who it's from, what it's about), then a one-sentence overall take. Plain words, no markdown.",
                Array.Empty<ChatMessage>(),
                sb.ToString());
            return string.IsNullOrWhiteSpace(summary)
                ? reply
                : "Here's what's in your inbox:\n\n" + summary.Trim();
        }
        catch
        {
            return reply;
        }
    }

    private async Task<string> DraftReplyAsync(string senderQuery, bool fromVoice)
    {
        var original = await _email.FindLatestFromAsync(senderQuery);
        if (original == null)
            return $"I couldn't find a recent email from anyone matching \"{senderQuery}\" in your unread mail.";

        var body = await _email.GetBodyAsync(original, 2000);
        string draft;
        if (_ai.IsAvailable)
        {
            try
            {
                var generated = await _ai.ChatAsync(
                    "Write a short, warm, professional email reply to the message below. " +
                    "Reply body only — no subject line, no greeting header, no sign-off name. " +
                    "Keep it to a few sentences.",
                    Array.Empty<ChatMessage>(),
                    $"From: {original.From}\nSubject: {original.Subject}\n\n{body}");
                draft = string.IsNullOrWhiteSpace(generated)
                    ? FallbackDraft(original)
                    : generated.Trim();
            }
            catch
            {
                draft = FallbackDraft(original);
            }
        }
        else
        {
            draft = FallbackDraft(original);
        }

        var copy = original; // captured for the confirmation closure
        var draftText = draft;
        _pendingConfirm = new PendingConfirmation
        {
            Prompt = "Send this reply?",
            RunAsync = async () =>
            {
                var (ok, error) = await _email.SendReplyAsync(copy, draftText);
                return ok
                    ? "Sent! Your reply is on its way."
                    : error;
            },
        };
        var voiceNote = fromVoice ? " (I'll read the draft, but the send needs your typed \"yes\".)" : string.Empty;
        return $"Here's a draft reply to {original.From} about \"{original.Subject}\" " +
               $"[{original.Provider}]:\n\n{draft}\n\nSay \"yes, send it\" and I'll send it.{voiceNote}";
    }

    private static string FallbackDraft(EmailMessage original) =>
        $"Thanks for your email about \"{original.Subject}\" — I've got it and I'll follow up shortly.";

    // ---------- Calendar ----------

    private async Task<string> CalendarTodayAsync()
    {
        var events = new List<CalendarEvent>();
        var problems = new List<string>();

        var g = await _gcal.GetTodayAsync();
        if (g.Ok) events.AddRange(g.Events);
        else if (!g.Error.Contains("isn't set up", StringComparison.OrdinalIgnoreCase)) problems.Add(g.Error);

        var o = await _outlookCal.GetTodayAsync();
        if (o.Ok) events.AddRange(o.Events);
        else if (!o.Error.Contains("isn't set up", StringComparison.OrdinalIgnoreCase)) problems.Add(o.Error);

        if (events.Count == 0 && problems.Count == 0)
            return "No calendar is connected yet. Say \"connect my gmail\" for Google Calendar or " +
                   "\"connect my outlook\" for your Outlook calendar.";

        events = events.OrderBy(e => e.Start).ToList();
        if (events.Count == 0)
        {
            var tail = problems.Count > 0 ? " " + string.Join(" ", problems) : string.Empty;
            return "Nothing on your calendar today — wide open!" + tail;
        }

        var sb = new StringBuilder("Here's your calendar today: ");
        sb.Append(string.Join("; ", events.Take(12).Select(e =>
        {
            var when = e.Start.Date == DateTime.Today && e.Start.TimeOfDay == TimeSpan.Zero
                ? "all day"
                : e.Start.ToString("h:mm tt");
            var where = string.IsNullOrWhiteSpace(e.Location) ? "" : $" at {e.Location}";
            return $"{e.Title} at {when} [{e.Provider}]{where}";
        })));
        sb.Append('.');
        return sb.ToString();
    }

    private async Task<string> ProposeEventAsync(string rest)
    {
        DateTime due;
        string title;
        try
        {
            // Reuse the reminder parser's natural-language time handling.
            (due, title) = _parser.Parse("remind me to " + rest);
        }
        catch
        {
            return "I couldn't work out when that is — try something like \"schedule dentist tomorrow at 3pm\".";
        }
        if (string.IsNullOrWhiteSpace(title))
            return "What should I call the event? Try \"schedule <name> <when>\".";

        var end = due.AddHours(1);

        // Pick the calendar: Google first, then Outlook. The confirmation names it.
        string provider = "Google Calendar";
        Func<Task<(bool Ok, string Error)>> create = async () =>
        {
            var r = await _gcal.CreateEventAsync(title, due, end);
            return (r.Ok, r.Error);
        };
        var gCheck = await _gcal.GetTodayAsync();
        if (!gCheck.Ok)
        {
            var oCheck = await _outlookCal.GetTodayAsync();
            if (oCheck.Ok)
            {
                provider = "Outlook calendar";
                create = async () =>
                {
                    var r = await _outlookCal.CreateEventAsync(title, due, end);
                    return r.Ok ? (true, string.Empty) : (false, r.Error);
                };
            }
            else
            {
                return "No calendar is connected yet. Say \"connect my gmail\" or \"connect my outlook\" first.";
            }
        }

        var capturedTitle = title;
        var capturedDue = due;
        var capturedEnd = end;
        var capturedProvider = provider;
        _pendingConfirm = new PendingConfirmation
        {
            Prompt = $"Save \"{title}\" on {provider}?",
            RunAsync = async () =>
            {
                var (ok, error) = await create();
                return ok
                    ? $"Done — \"{capturedTitle}\" is on your {capturedProvider} for {capturedDue:MMMM d} at {capturedDue:h:mm tt}."
                    : error;
            },
        };
        return $"Here's what I'd put on your calendar: \"{title}\", {due:dddd, MMMM d} at {due:h:mm tt} " +
               $"– {end:h:mm tt} ({provider}). Say \"yes\" to save it.";
    }

    // ---------- Family memory book ----------

    /// <summary>
    /// Handles "tell me a memory", "tell me about mom", "do you remember
    /// when ...". Returns null when the input isn't a memory request (caller
    /// falls through to web lookup / AI). When the book isn't installed or
    /// the toggle is off, answers honestly instead of letting the AI invent
    /// memories.
    /// </summary>
    private string? TryHandleMemoryBook(string text, string lower)
    {
        bool isRandom = Regex.IsMatch(lower, @"\b(tell me a memory|share a memory|tell me a story)\b");
        var aboutMatch = Regex.Match(text,
            @"^(please\s+)?(tell me about|what do you remember about)\s+(.+)$",
            RegexOptions.IgnoreCase);
        var whenMatch = Regex.Match(text,
            @"^(do you )?remember when\s+(.+)$",
            RegexOptions.IgnoreCase);

        var book = MemoryBookService.Instance;
        bool isAboutKnownPerson = aboutMatch.Success &&
            book.FindPerson(aboutMatch.Groups[3].Value) != null;

        if (!isRandom && !isAboutKnownPerson && !whenMatch.Success)
            return null; // not a memory request — fall through

        if (!book.IsAvailable || !_settings.MemoryBookEnabled)
            return "I don't have Shaun's family memory book on this PC, so I can't share " +
                   "his memories. If a memory book is added later, ask me again and I'll " +
                   "tell you everything in it.";

        if (isRandom)
            return ShareRandomMemory(book);
        if (isAboutKnownPerson)
            return SharePersonMemories(book, aboutMatch.Groups[3].Value);
        return ShareSearchedMemories(book, whenMatch.Groups[2].Value);
    }

    private static string ShareRandomMemory(MemoryBookService book)
    {
        var match = book.RandomMemory();
        if (match == null)
            return "The memory book seems to be empty — nothing to share yet.";
        return "Here's one of Shaun's memories. " + FormatMemory(match);
    }

    private static string SharePersonMemories(MemoryBookService book, string personQuery)
    {
        var matches = book.MemoriesFor(personQuery);
        if (matches.Count == 0)
            return "I couldn't find memories with them in Shaun's memory book.";
        var person = matches[0].Person;
        var sb = new StringBuilder(
            $"Here are Shaun's memories with {person.Name} ({person.Relationship}): ");
        foreach (var m in matches.Take(3))
        {
            sb.Append(FormatMemory(m));
            sb.Append(' ');
        }
        if (matches.Count > 3)
            sb.Append($"...and {matches.Count - 3} more in the book.");
        return sb.ToString().Trim();
    }

    private static string ShareSearchedMemories(MemoryBookService book, string query)
    {
        var matches = book.SearchMemories(query).Take(3).ToList();
        if (matches.Count == 0)
        {
            var names = string.Join(", ", book.People.Select(p => p.Name));
            return "That's not in Shaun's memory book — I don't want to make anything up. " +
                   $"Ask me about {names}, and I'll share what I have.";
        }
        var sb = new StringBuilder("Yes — here's what Shaun's memory book has: ");
        foreach (var m in matches)
        {
            sb.Append(FormatMemory(m));
            sb.Append(' ');
        }
        return sb.ToString().Trim();
    }

    private static string FormatMemory(MemoryMatch m)
    {
        var date = FormatMemoryDate(m.Entry.Date);
        var prefix = string.IsNullOrEmpty(date) ? string.Empty : $"Back on {date}, ";
        return $"{prefix}{m.Entry.Title}: {m.Entry.Text}";
    }

    private static string FormatMemoryDate(string raw)
    {
        try
        {
            if (DateTime.TryParse(raw, out var d))
                return d.ToString("MMMM d, yyyy");
        }
        catch
        {
        }
        return raw ?? string.Empty;
    }

    // ---------- Multi-step planning ----------

    private async Task<string> PlanDayAsync(bool tomorrow)
    {
        try
        {
            var day = tomorrow ? DateTime.Today.AddDays(1) : DateTime.Today;
            var reminders = await _reminders.GetDueOnAsync(day);
            var todos = await _todos.GetOpenAsync();

            var blocks = new List<PlanBlock>();
            foreach (var r in reminders)
                blocks.Add(new PlanBlock { Text = r.Text, At = r.DueAt });

            // Fit open todos into sensible slots after 9:30 AM, skipping
            // times already taken by reminders.
            var slot = day.AddHours(9).AddMinutes(30);
            if (!tomorrow && slot < DateTime.Now.AddMinutes(30))
                slot = DateTime.Now.AddMinutes(30);
            foreach (var t in todos.Take(6))
            {
                while (blocks.Any(b => Math.Abs((b.At - slot).TotalMinutes) < 45))
                    slot = slot.AddHours(1);
                blocks.Add(new PlanBlock { Text = t.Text, At = slot });
                slot = slot.AddHours(1);
            }

            blocks = blocks.OrderBy(b => b.At).ToList();

            if (blocks.Count == 0)
            {
                _pendingPlan = null;
                return tomorrow
                    ? "Tomorrow's wide open — no reminders, no todos. Want to put something on it?"
                    : "Your morning's wide open — no reminders, no todos. Enjoy the quiet, or tell me something to add!";
            }

            _pendingPlan = blocks;
            var sb = new StringBuilder();
            sb.Append(tomorrow ? "Here's your plan for tomorrow: " : "Here's your morning: ");
            sb.Append(string.Join(". ", blocks.Select(b => $"at {b.At:h:mm tt}, {b.Text}")));
            sb.Append(". Want me to set reminders for each of these?");
            return sb.ToString();
        }
        catch (Exception)
        {
            _pendingPlan = null;
            return "I couldn't put that plan together just now.";
        }
    }

    private async Task<string> CreatePlanRemindersAsync(List<PlanBlock> blocks)
    {
        try
        {
            int created = 0;
            var existing = await _reminders.GetUpcomingAsync();
            foreach (var b in blocks)
            {
                // Don't duplicate a reminder that already covers this.
                if (existing.Any(r => r.Text.Equals(b.Text, StringComparison.OrdinalIgnoreCase)))
                    continue;
                await _reminders.AddAsync(new ReminderItem { Text = b.Text, DueAt = b.At });
                created++;
            }
            return created == 0
                ? "Looks like those are all covered already — you're set!"
                : $"Done! I've set {created} reminder{(created == 1 ? "" : "s")} for your plan. Go get 'em!";
        }
        catch (Exception)
        {
            return "I ran into trouble setting those reminders.";
        }
    }

    // ---------- Matching helper ----------

    private static T? FindBestMatch<T>(IReadOnlyList<T> items, Func<T, string> textOf, string target)
        where T : class
    {
        var keywords = target.ToLowerInvariant()
            .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !StopWords.Contains(w))
            .ToList();
        if (keywords.Count == 0)
            return null;

        T? best = null;
        int bestScore = 0;
        foreach (var item in items)
        {
            var haystack = textOf(item).ToLowerInvariant();
            int score = keywords.Count(k => haystack.Contains(k, StringComparison.Ordinal));
            if (score > bestScore)
            {
                bestScore = score;
                best = item;
            }
        }
        return bestScore > 0 ? best : null;
    }
}
