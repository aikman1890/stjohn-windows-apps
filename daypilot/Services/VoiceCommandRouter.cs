using System.Text.RegularExpressions;
using DayPilot.Models;
using Windows.System;

namespace DayPilot.Services;

/// <summary>
/// What the voice assistant decided to do with one utterance.
/// </summary>
public sealed class VoiceReply
{
    /// <summary>Short, warm response — spoken aloud and shown on screen.</summary>
    public string Spoken { get; init; } = string.Empty;

    /// <summary>
    /// Optional longer on-screen detail (lists, full answers). Defaults to
    /// <see cref="Spoken"/> when not set.
    /// </summary>
    public string? Detail { get; init; }

    public string Display => Detail ?? Spoken;
}

/// <summary>
/// Routes transcribed speech to DayPilot features: reminders, notes,
/// to-dos, day summaries, web lookups, and the on-device AI fallback.
/// Every reply is short, warm, and upbeat — written to be spoken.
/// </summary>
public sealed class VoiceCommandRouter
{
    private readonly ReminderParser _parser = new();
    private readonly ReminderStore _reminders = new();
    private readonly NoteStore _notes = new();
    private readonly TodoStore _todos = new();
    private readonly AiService _ai = AiService.Instance;

    public async Task<VoiceReply> HandleAsync(string input)
    {
        string text = (input ?? string.Empty).Trim().TrimEnd('.', '!', '?');
        if (string.IsNullOrWhiteSpace(text))
            return new VoiceReply { Spoken = "I didn't catch that — say it once more for me?" };

        try
        {
            // 1. Reminders — "remind me to ...", "remind me about ... tomorrow".
            if (Regex.IsMatch(text, @"\bremind\s+me\b", RegexOptions.IgnoreCase))
                return await HandleReminderAsync(text);

            // 2. Notes — "take a note ...", "note that ...", "jot this down ...".
            var noteMatch = Regex.Match(text,
                @"^(?:please\s+)?(?:take\s+a\s+note|take\s+note|note\s+that|add\s+a\s+note|write\s+(?:this|that|it\s+)?down|jot\s+(?:this|that|it\s+)?down)\b[\s,]+(.+)$",
                RegexOptions.IgnoreCase);
            if (noteMatch.Success)
                return await HandleNoteAsync(noteMatch.Groups[1].Value.Trim());

            // 3a. To-do completion — "complete ...", "mark ... done", "finish ...".
            var doneMatch = Regex.Match(text, @"^mark\s+(.+?)\s+done$",
                RegexOptions.IgnoreCase);
            if (!doneMatch.Success)
                doneMatch = Regex.Match(text, @"^(?:complete|finish|check\s+off|done\s+with)\s+(.+)$",
                    RegexOptions.IgnoreCase);
            if (doneMatch.Success)
                return await HandleTodoCompleteAsync(doneMatch.Groups[1].Value.Trim());

            // 3b. To-do list — "what are my todos", "show my tasks".
            if (Regex.IsMatch(text,
                @"\b(my\s+)?(todo|to-do|task)\s?(list|s)\b.*\b(show|list|what|read|display)\b|\b(what|show|list|read)\b.*\bmy\s+(todos?|to-dos?|tasks?)\b",
                RegexOptions.IgnoreCase))
                return await HandleTodoListAsync();

            // 3c. To-do add — "add a todo ...", "add a task ...".
            var todoMatch = Regex.Match(text,
                @"^\s*add\s+(?:a\s+|another\s+)?(?:todo|to-do|task)(?:\s+item)?\s+(.+)$",
                RegexOptions.IgnoreCase);
            if (todoMatch.Success)
                return await HandleTodoAddAsync(todoMatch.Groups[1].Value.Trim());

            // 4. Day questions — "what's my day look like", "what's next".
            if (Regex.IsMatch(text,
                @"what'?s\s+(my\s+day|on\s+(today|my\s+calendar|the\s+agenda)|next|happening|coming\s+up)|what\s+do\s+i\s+have\s+(today|on)|how'?s\s+my\s+day|anything\s+(due|on)\s+today",
                RegexOptions.IgnoreCase))
                return await HandleDaySummaryAsync();

            // 5. Web lookups — "look up ...", "search for ...", "find ...".
            var webMatch = Regex.Match(text,
                @"^(?:look\s+up|search(?:\s+for)?|google|find|look\s+for)\s+(.+)$",
                RegexOptions.IgnoreCase);
            if (webMatch.Success)
                return await HandleWebLookupAsync(webMatch.Groups[1].Value.Trim());

            // 6. On-device AI fallback.
            return await HandleAiFallbackAsync(text);
        }
        catch (Exception)
        {
            return new VoiceReply
            {
                Spoken = "Hmm, something hiccupped on my end. Mind saying that once more?",
            };
        }
    }

    private async Task<VoiceReply> HandleReminderAsync(string text)
    {
        var (due, reminderText) = _parser.Parse(text);
        var item = new ReminderItem { Text = reminderText, DueAt = due };
        await _reminders.AddAsync(item);
        NotificationService.Instance.ShowToast("Reminder set", $"{reminderText} — {due:g}");

        string when = FriendlyWhen(due);
        return new VoiceReply
        {
            Spoken = $"Got it! I'll remind you about {reminderText} {when}.",
            Detail = $"Reminder set: \"{reminderText}\" — due {due:dddd, MMMM d} at {due:h:mm tt}.",
        };
    }

    private async Task<VoiceReply> HandleNoteAsync(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return new VoiceReply { Spoken = "What would you like the note to say?" };

        string title = body.Length <= 40 ? body : body.Substring(0, 40).TrimEnd() + "…";
        await _notes.AddAsync(new Note { Title = title, Body = body });
        return new VoiceReply
        {
            Spoken = "Done — saved that to your notes!",
            Detail = $"Note saved: \"{title}\"",
        };
    }

    private async Task<VoiceReply> HandleTodoAddAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new VoiceReply { Spoken = "What should I put on your task list?" };

        await _todos.AddAsync(new TodoItem { Title = title });
        return new VoiceReply
        {
            Spoken = "Done — that's on your list!",
            Detail = $"Task added: \"{title}\"",
        };
    }

    private async Task<VoiceReply> HandleTodoListAsync()
    {
        var open = (await _todos.GetAllAsync())
            .Where(t => !t.IsDone)
            .OrderBy(t => t.CreatedAt)
            .ToList();

        if (open.Count == 0)
            return new VoiceReply { Spoken = "Your task list is all clear. Nicely done!" };

        var spokenNames = open.Take(5).Select(t => t.Title).ToList();
        string spokenList = spokenNames.Count == 1
            ? spokenNames[0]
            : string.Join(", ", spokenNames.Take(spokenNames.Count - 1)) + ", and " + spokenNames[^1];
        string spoken = open.Count == 1
            ? $"You have one open task: {spokenList}."
            : $"You have {open.Count} open tasks: {spokenList}.";
        if (open.Count > 5)
            spoken += " I've put the full list on screen.";

        string detail = "Open tasks:\n" + string.Join("\n",
            open.Select((t, i) => $"{i + 1}. {t.Title}"));
        return new VoiceReply { Spoken = spoken, Detail = detail };
    }

    private async Task<VoiceReply> HandleTodoCompleteAsync(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return new VoiceReply { Spoken = "Which task should I check off?" };

        var open = (await _todos.GetAllAsync()).Where(t => !t.IsDone).ToList();
        if (open.Count == 0)
            return new VoiceReply { Spoken = "Your task list is already clear — nothing to check off!" };

        string needle = target.ToLowerInvariant();
        var matches = open.Where(t =>
                t.Title.ToLowerInvariant().Contains(needle) ||
                needle.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 2)
                    .All(w => t.Title.Contains(w, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (matches.Count == 1)
        {
            var done = matches[0];
            done.IsDone = true;
            done.CompletedAt = DateTime.Now;
            await _todos.UpdateAsync(done);
            return new VoiceReply
            {
                Spoken = $"Done — checked off {done.Title}!",
                Detail = $"Completed: \"{done.Title}\"",
            };
        }

        if (matches.Count == 0)
            return new VoiceReply
            {
                Spoken = "Hmm, I couldn't find a task matching that. Here's your list —",
                Detail = "No match. Open tasks:\n" + string.Join("\n",
                    open.Select((t, i) => $"{i + 1}. {t.Title}")),
            };

        return new VoiceReply
        {
            Spoken = "I found a few that match — take a look and be a bit more specific?",
            Detail = "Multiple matches:\n" + string.Join("\n",
                matches.Select((t, i) => $"{i + 1}. {t.Title}")),
        };
    }

    private async Task<VoiceReply> HandleDaySummaryAsync()
    {
        var now = DateTime.Now;
        var today = now.Date;
        var reminders = (await _reminders.GetAllAsync()).Where(r => !r.IsDone).ToList();
        var dueToday = reminders
            .Where(r => r.DueAt.Date == today && r.DueAt >= now)
            .OrderBy(r => r.DueAt)
            .ToList();
        var overdue = reminders.Count(r => r.DueAt < now);
        var openTodos = (await _todos.GetAllAsync()).Count(t => !t.IsDone);

        var parts = new List<string>();
        if (dueToday.Count == 0)
            parts.Add("nothing else due today");
        else if (dueToday.Count == 1)
            parts.Add($"one thing due today: {dueToday[0].Text} at {dueToday[0].DueAt:h:mm tt}");
        else
            parts.Add($"{dueToday.Count} things due today, starting with {dueToday[0].Text} at {dueToday[0].DueAt:h:mm tt}");
        parts.Add(overdue == 0 ? "you're all caught up on overdue items"
            : overdue == 1 ? "one overdue item to catch up on"
            : $"{overdue} overdue items to catch up on");
        parts.Add(openTodos == 0 ? "and your task list is clear"
            : openTodos == 1 ? "and one open task"
            : $"and {openTodos} open tasks");

        string spoken = "Here's your day: " + string.Join(", ", parts) + ". You've got this!";
        string detail = $"Due today ({dueToday.Count}):\n" +
            (dueToday.Count == 0 ? "  —\n" : string.Join("\n", dueToday.Select(r => $"  • {r.Text} — {r.DueAt:h:mm tt}")) + "\n") +
            $"Overdue: {overdue}\nOpen tasks: {openTodos}";
        return new VoiceReply { Spoken = spoken, Detail = detail };
    }

    private async Task<VoiceReply> HandleWebLookupAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new VoiceReply { Spoken = "What would you like me to look up?" };

        try
        {
            var uri = new Uri("https://www.bing.com/search?q=" + Uri.EscapeDataString(query));
            await Launcher.LaunchUriAsync(uri);
        }
        catch (Exception)
        {
            return new VoiceReply
            {
                Spoken = "I couldn't open your browser just now — but you can search for " + query + " yourself.",
            };
        }

        return new VoiceReply
        {
            Spoken = "Looking that up for you now!",
            Detail = $"Opened a web search for \"{query}\" in your browser.",
        };
    }

    private async Task<VoiceReply> HandleAiFallbackAsync(string text)
    {
        await _ai.InitializeAsync();
        if (!_ai.IsAvailable)
        {
            return new VoiceReply
            {
                Spoken = "I'm not sure about that one — but I can set reminders, take notes, manage your task list, or look things up on the web!",
            };
        }

        string answer = await _ai.ChatAsync(text);
        return new VoiceReply { Spoken = answer };
    }

    private static string FriendlyWhen(DateTime due)
    {
        var now = DateTime.Now;
        string time = due.ToString("h:mm tt");
        if (due.Date == now.Date)
            return "today at " + time;
        if (due.Date == now.Date.AddDays(1))
            return "tomorrow at " + time;
        return "on " + due.ToString("dddd") + " at " + time;
    }
}
