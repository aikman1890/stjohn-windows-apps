using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Identity.Client;

namespace Wuse.Services;

/// <summary>
/// Outlook / Hotmail / Office 365 mail via Microsoft Graph REST (v1.0).
/// One instance fans out across every connected Microsoft account.
/// </summary>
public sealed class OutlookMailClient
{
    private readonly MicrosoftAuthService _auth;
    private static readonly HttpClient Http = new();

    public OutlookMailClient(MicrosoftAuthService auth) => _auth = auth;

    public async Task<(bool Ok, string Error, IReadOnlyList<EmailMessage> Messages)> GetUnreadAsync(int maxPerAccount = 10)
    {
        if (!_auth.IsConfigured)
            return (false, MicrosoftAuthService.MicrosoftSetupMessage, Array.Empty<EmailMessage>());

        var accounts = await _auth.GetAccountsAsync();
        if (accounts.Count == 0)
            return (false,
                "No Microsoft account is connected yet. Say \"connect my outlook\" and I'll open " +
                "the Microsoft sign-in page — it works for Hotmail, Outlook.com, and Office 365 work accounts.",
                Array.Empty<EmailMessage>());

        var all = new List<EmailMessage>();
        var errors = new List<string>();
        foreach (var account in accounts)
        {
            var token = await _auth.GetTokenSilentAsync(account);
            if (token == null)
            {
                errors.Add($"{MicrosoftAuthService.AccountLabel(account)} needs you to sign in again.");
                continue;
            }
            var (ok, err, msgs) = await GetUnreadForAccountAsync(token, account, maxPerAccount);
            if (!ok)
                errors.Add(err);
            else
                all.AddRange(msgs);
        }

        all = all.OrderByDescending(m => m.Received).ToList();
        if (all.Count == 0 && errors.Count > 0)
            return (false, string.Join(" ", errors), all);
        return (true, string.Empty, all);
    }

    public async Task<string> GetBodyAsync(EmailMessage message, int maxChars = 4000)
    {
        try
        {
            var accounts = await _auth.GetAccountsAsync();
            var account = accounts.FirstOrDefault(a =>
                MicrosoftAuthService.AccountLabel(a) == message.AccountLabel);
            if (account == null)
                return string.Empty;
            var token = await _auth.GetTokenSilentAsync(account);
            if (token == null)
                return string.Empty;

            using var req = new HttpRequestMessage(HttpMethod.Get,
                $"https://graph.microsoft.com/v1.0/me/messages/{message.Id}?$select=body");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var resp = await Http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return string.Empty;
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var content = doc.RootElement.GetProperty("body").GetProperty("content").GetString() ?? string.Empty;
            var text = StripHtml(content).Trim();
            return text.Length > maxChars ? text.Substring(0, maxChars) + "…" : text;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<(bool Ok, string Error)> SendAsync(EmailMessage inReplyTo, string toAddress, string subject, string bodyText)
    {
        try
        {
            var accounts = await _auth.GetAccountsAsync();
            var account = accounts.FirstOrDefault(a =>
                MicrosoftAuthService.AccountLabel(a) == inReplyTo.AccountLabel);
            if (account == null)
                return (false, "I lost track of which account that draft was for — mind starting the draft again?");
            var token = await _auth.GetTokenSilentAsync(account);
            if (token == null)
                return (false, "That Microsoft account needs you to sign in again first.");

            var payload = new
            {
                message = new
                {
                    subject,
                    body = new { contentType = "Text", content = bodyText },
                    toRecipients = new[] { new { emailAddress = new { address = toAddress } } },
                },
            };
            var json = JsonSerializer.Serialize(payload);
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/me/sendMail")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var resp = await Http.SendAsync(req);
            return resp.IsSuccessStatusCode
                ? (true, string.Empty)
                : (false, $"Outlook wouldn't send it (server said {(int)resp.StatusCode}).");
        }
        catch (Exception ex)
        {
            return (false, $"Outlook wouldn't send it: {ex.Message}");
        }
    }

    private static async Task<(bool Ok, string Error, IReadOnlyList<EmailMessage> Messages)> GetUnreadForAccountAsync(
        string token, IAccount account, int max)
    {
        try
        {
            var label = MicrosoftAuthService.AccountLabel(account);
            var url = "https://graph.microsoft.com/v1.0/me/messages" +
                      "?$filter=isRead eq false" +
                      "&$top=" + max +
                      "&$select=id,subject,from,receivedDateTime,bodyPreview" +
                      "&$orderby=receivedDateTime desc";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var resp = await Http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return (false, $"Outlook ({label}) returned {(int)resp.StatusCode}.", Array.Empty<EmailMessage>());

            var messages = new List<EmailMessage>();
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            foreach (var item in doc.RootElement.GetProperty("value").EnumerateArray())
            {
                messages.Add(new EmailMessage
                {
                    Provider = "Outlook",
                    AccountLabel = label,
                    Id = GetString(item, "id"),
                    From = GetEmailAddress(item, "from"),
                    Subject = GetString(item, "subject") is string s && s.Length > 0 ? s : "(no subject)",
                    Snippet = GetString(item, "bodyPreview"),
                    Received = GetDateTime(item, "receivedDateTime"),
                });
            }
            return (true, string.Empty, messages);
        }
        catch (Exception ex)
        {
            return (false, $"I couldn't reach that Outlook account: {ex.Message}", Array.Empty<EmailMessage>());
        }
    }

    private static string GetString(JsonElement el, string name) =>
        el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString() ?? string.Empty
            : string.Empty;

    private static string GetEmailAddress(JsonElement el, string name)
    {
        try
        {
            var addr = el.GetProperty(name).GetProperty("emailAddress");
            var display = addr.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
            var email = addr.TryGetProperty("address", out var a) ? a.GetString() ?? "" : "";
            if (display.Length > 0 && email.Length > 0 && !display.Equals(email, StringComparison.OrdinalIgnoreCase))
                return $"{display} <{email}>";
            return email.Length > 0 ? email : display;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static DateTime GetDateTime(JsonElement el, string name)
    {
        var s = GetString(el, name);
        if (DateTimeOffset.TryParse(s, out var dto))
            return dto.LocalDateTime;
        return DateTime.Now;
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;
        var text = Regex.Replace(html, "<[^>]+>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        return Regex.Replace(text, @"\s+", " ");
    }
}
