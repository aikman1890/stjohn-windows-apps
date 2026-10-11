using System.Text;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;

namespace Wuse.Services;

/// <summary>
/// A normalized email message, provider-agnostic.
/// </summary>
public sealed class EmailMessage
{
    public string Provider { get; set; } = string.Empty;   // "Gmail", "Outlook", "Yahoo"
    public string AccountLabel { get; set; } = string.Empty; // which connected account
    public string Id { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Subject { get; set; } = "(no subject)";
    public string Snippet { get; set; } = string.Empty;
    public DateTime Received { get; set; }
}

/// <summary>
/// A normalized calendar event, provider-agnostic.
/// </summary>
public sealed class CalendarEvent
{
    public string Provider { get; set; } = string.Empty;
    public string AccountLabel { get; set; } = string.Empty;
    public string Title { get; set; } = "(no title)";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Location { get; set; } = string.Empty;
}

/// <summary>
/// Gmail via Google's .NET client library. Read, send, and body fetch.
/// </summary>
public sealed class GmailClient
{
    private readonly GoogleAuthService _auth;

    public GmailClient(GoogleAuthService auth) => _auth = auth;

    public async Task<(bool Ok, string Error, IReadOnlyList<EmailMessage> Messages)> GetUnreadAsync(int max = 10)
    {
        var (ok, error, cred) = await _auth.EnsureCredentialAsync();
        if (!ok || cred == null)
            return (false, error, Array.Empty<EmailMessage>());

        try
        {
            var service = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = "wuse",
            });
            var listReq = service.Users.Messages.List("me");
            listReq.Q = "is:unread";
            listReq.MaxResults = max;
            var list = await listReq.ExecuteAsync();

            var messages = new List<EmailMessage>();
            if (list.Messages != null)
            {
                foreach (var m in list.Messages)
                {
                    var getReq = service.Users.Messages.Get("me", m.Id);
                    getReq.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Metadata;
                    getReq.MetadataHeaders = new[] { "From", "Subject", "Date" };
                    var full = await getReq.ExecuteAsync();
                    messages.Add(new EmailMessage
                    {
                        Provider = "Gmail",
                        AccountLabel = "Gmail",
                        Id = m.Id ?? string.Empty,
                        From = Header(full, "From"),
                        Subject = Header(full, "Subject") is string s && s.Length > 0 ? s : "(no subject)",
                        Snippet = full.Snippet ?? string.Empty,
                        Received = ParseDate(Header(full, "Date")),
                    });
                }
            }
            return (true, string.Empty, messages);
        }
        catch (Exception ex)
        {
            return (false, $"I couldn't reach Gmail: {ex.Message}", Array.Empty<EmailMessage>());
        }
    }

    public async Task<string> GetBodyAsync(string messageId, int maxChars = 4000)
    {
        var (ok, _, cred) = await _auth.EnsureCredentialAsync();
        if (!ok || cred == null)
            return string.Empty;
        try
        {
            var service = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = "wuse",
            });
            var getReq = service.Users.Messages.Get("me", messageId);
            getReq.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
            var msg = await getReq.ExecuteAsync();
            var body = ExtractPlainText(msg.Payload);
            return body.Length > maxChars ? body.Substring(0, maxChars) + "…" : body;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<(bool Ok, string Error)> SendAsync(string to, string subject, string bodyText)
    {
        var (ok, error, cred) = await _auth.EnsureCredentialAsync();
        if (!ok || cred == null)
            return (false, error);
        try
        {
            var service = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = "wuse",
            });
            var mime = new StringBuilder();
            mime.AppendLine($"To: {to}");
            mime.AppendLine($"Subject: {subject}");
            mime.AppendLine("Content-Type: text/plain; charset=utf-8");
            mime.AppendLine();
            mime.AppendLine(bodyText);
            var raw = Base64UrlEncode(Encoding.UTF8.GetBytes(mime.ToString()));
            await service.Users.Messages.Send(new Message { Raw = raw }, "me").ExecuteAsync();
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, $"Gmail wouldn't send it: {ex.Message}");
        }
    }

    private static string Header(Google.Apis.Gmail.v1.Data.Message msg, string name)
    {
        var headers = msg.Payload?.Headers;
        if (headers == null)
            return string.Empty;
        foreach (var h in headers)
            if (string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase))
                return h.Value ?? string.Empty;
        return string.Empty;
    }

    private static DateTime ParseDate(string value)
    {
        if (DateTimeOffset.TryParse(value, out var dto))
            return dto.LocalDateTime;
        return DateTime.Now;
    }

    private static string ExtractPlainText(MessagePart? part)
    {
        if (part == null)
            return string.Empty;
        if (string.Equals(part.MimeType, "text/plain", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(part.Body?.Data))
        {
            try { return Encoding.UTF8.GetString(Base64UrlDecode(part.Body.Data)); }
            catch { return string.Empty; }
        }
        if (part.Parts != null)
        {
            foreach (var sub in part.Parts)
            {
                var text = ExtractPlainText(sub);
                if (text.Length > 0)
                    return text;
            }
        }
        return string.Empty;
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
