using System.Text;

namespace Wuse.Services;

/// <summary>
/// Unified email across Gmail, Outlook/Hotmail/Office 365, and Yahoo.
/// "check my email" aggregates unread from every connected account, labeled
/// by account. One account down never breaks the others. Email bodies are
/// the user's private data: fetched for summaries/drafts, never logged.
/// </summary>
public sealed class EmailAggregator
{
    private readonly GmailClient _gmail;
    private readonly OutlookMailClient _outlook;
    private readonly YahooMailClient _yahoo;

    public EmailAggregator(GmailClient gmail, OutlookMailClient outlook, YahooMailClient yahoo)
    {
        _gmail = gmail;
        _outlook = outlook;
        _yahoo = yahoo;
    }

    public async Task<(bool AnyConnected, string Reply)> CheckAllAsync(int maxPerAccount = 8)
    {
        var all = new List<EmailMessage>();
        var problems = new List<string>();
        bool anyConfigured = false;

        var g = await _gmail.GetUnreadAsync(maxPerAccount);
        if (g.Ok) { anyConfigured = true; all.AddRange(g.Messages); }
        else if (!g.Error.Contains("isn't set up", StringComparison.OrdinalIgnoreCase)) problems.Add(g.Error);

        var o = await _outlook.GetUnreadAsync(maxPerAccount);
        if (o.Ok) { anyConfigured = true; all.AddRange(o.Messages); }
        else if (!o.Error.Contains("isn't set up", StringComparison.OrdinalIgnoreCase)) problems.Add(o.Error);

        var y = await _yahoo.GetUnreadAsync(maxPerAccount);
        if (y.Ok) { anyConfigured = true; all.AddRange(y.Messages); }
        else if (!y.Error.Contains("isn't connected", StringComparison.OrdinalIgnoreCase)) problems.Add(y.Error);

        if (!anyConfigured)
            return (false,
                "No email is connected yet. I can hook up Gmail, Outlook/Hotmail/Office 365, " +
                "and Yahoo — say \"connect my gmail\", \"connect my outlook\", or \"connect my yahoo\" " +
                "to get started.");

        all = all.OrderByDescending(m => m.Received).ToList();
        if (all.Count == 0)
        {
            var tail = problems.Count > 0 ? " " + string.Join(" ", problems) : string.Empty;
            return (true, "All quiet — no unread mail anywhere." + tail);
        }

        var sb = new StringBuilder();
        sb.Append($"You've got {all.Count} unread across your accounts: ");
        sb.Append(string.Join("; ", all.Take(12).Select(m =>
            $"[{m.Provider}] {m.From}: {m.Subject}")));
        if (all.Count > 12)
            sb.Append($" …and {all.Count - 12} more.");
        if (problems.Count > 0)
            sb.Append(" (Note: " + string.Join(" ", problems) + ")");
        return (true, sb.ToString());
    }

    public Task<(bool Ok, string Error, IReadOnlyList<EmailMessage> Messages)> CheckGmailAsync(int max = 10) =>
        _gmail.GetUnreadAsync(max);

    public Task<(bool Ok, string Error, IReadOnlyList<EmailMessage> Messages)> CheckOutlookAsync(int max = 10) =>
        _outlook.GetUnreadAsync(max);

    public Task<(bool Ok, string Error, IReadOnlyList<EmailMessage> Messages)> CheckYahooAsync(int max = 10) =>
        _yahoo.GetUnreadAsync(max);

    /// <summary>
    /// Finds the newest message (read or unread) from someone matching the
    /// name/address, across all connected accounts. Searches recent unread
    /// first, then falls back gracefully.
    /// </summary>
    public async Task<EmailMessage?> FindLatestFromAsync(string senderQuery)
    {
        var q = (senderQuery ?? string.Empty).Trim().ToLowerInvariant();
        if (q.Length < 2)
            return null;

        var candidates = new List<EmailMessage>();
        var g = await _gmail.GetUnreadAsync(25);
        if (g.Ok) candidates.AddRange(g.Messages);
        var o = await _outlook.GetUnreadAsync(25);
        if (o.Ok) candidates.AddRange(o.Messages);
        var y = await _yahoo.GetUnreadAsync(25);
        if (y.Ok) candidates.AddRange(y.Messages);

        var words = q.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2).ToList();
        return candidates
            .Select(m => (Msg: m, Score: ScoreSender(m.From.ToLowerInvariant(), q, words)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Msg.Received)
            .Select(x => x.Msg)
            .FirstOrDefault();
    }

    public async Task<string> GetBodyAsync(EmailMessage message, int maxChars = 4000) =>
        message.Provider switch
        {
            "Gmail" => await _gmail.GetBodyAsync(message.Id, maxChars),
            "Outlook" => await _outlook.GetBodyAsync(message, maxChars),
            "Yahoo" => await _yahoo.GetBodyAsync(message, maxChars),
            _ => string.Empty,
        };

    /// <summary>
    /// Sends a reply from the same account the original arrived on.
    /// Yahoo is read-only — returns a graceful refusal for Yahoo drafts.
    /// </summary>
    public async Task<(bool Ok, string Error)> SendReplyAsync(EmailMessage inReplyTo, string bodyText)
    {
        var subject = inReplyTo.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase)
            ? inReplyTo.Subject
            : "Re: " + inReplyTo.Subject;
        var to = ExtractAddress(inReplyTo.From);

        return inReplyTo.Provider switch
        {
            "Gmail" => await _gmail.SendAsync(to, subject, bodyText),
            "Outlook" => await _outlook.SendAsync(inReplyTo, to, subject, bodyText),
            "Yahoo" => (false,
                "Yahoo is connected read-only, so I can't send from it — but the draft is " +
                "ready to copy into Yahoo Mail yourself."),
            _ => (false, "I don't know how to send from that account."),
        };
    }

    private static int ScoreSender(string from, string query, List<string> words)
    {
        if (from.Contains(query, StringComparison.Ordinal))
            return 100;
        int hits = words.Count(w => from.Contains(w, StringComparison.Ordinal));
        return hits == words.Count && hits > 0 ? 50 : 0;
    }

    private static string ExtractAddress(string from)
    {
        // "Display Name <addr@x.com>" → addr@x.com
        var start = from.LastIndexOf('<');
        var end = from.LastIndexOf('>');
        if (start >= 0 && end > start)
            return from.Substring(start + 1, end - start - 1).Trim();
        return from.Trim();
    }
}
