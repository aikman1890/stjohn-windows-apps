using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;

namespace Wuse.Services;

/// <summary>
/// Yahoo Mail, read-only over IMAP (MailKit). Auth is a Yahoo app-specific
/// password: the user makes one at login.yahoo.com → Account Security, enters
/// it once, and wuse keeps it encrypted in the Credential-Locker vault —
/// never in a file. Bodies are fetched only for summaries; sending from Yahoo
/// is not supported (drafts can still be prepared and shown).
/// </summary>
public sealed class YahooMailClient
{
    private const string ImapHost = "imap.mail.yahoo.com";
    private const int ImapPort = 993;

    private readonly VaultService _vault;

    public YahooMailClient(VaultService vault) => _vault = vault;

    public bool IsConfigured => _vault.GetServiceCredential(VaultService.YahooCredentialName) != null;

    public async Task<(bool Ok, string Error, IReadOnlyList<EmailMessage> Messages)> GetUnreadAsync(int max = 10)
    {
        var cred = _vault.GetServiceCredential(VaultService.YahooCredentialName);
        if (cred == null)
            return (false,
                "Yahoo isn't connected yet. Make an app password at login.yahoo.com → Account " +
                "Security → \"Generate app password\", then say \"connect my yahoo\" and I'll " +
                "walk you through it — it gets stored encrypted, never in a file.",
                Array.Empty<EmailMessage>());

        var (email, appPassword) = cred.Value;
        using var client = new ImapClient();
        try
        {
            await client.ConnectAsync(ImapHost, ImapPort, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(email, appPassword);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            var uids = await inbox.SearchAsync(SearchQuery.NotSeen);

            var messages = new List<EmailMessage>();
            foreach (var uid in uids.TakeLast(Math.Min(max, 25)).Reverse())
            {
                try
                {
                    var mime = await inbox.GetMessageAsync(uid);
                    messages.Add(new EmailMessage
                    {
                        Provider = "Yahoo",
                        AccountLabel = $"Yahoo ({email})",
                        Id = uid.ToString(),
                        From = mime.From.ToString(),
                        Subject = string.IsNullOrWhiteSpace(mime.Subject) ? "(no subject)" : mime.Subject,
                        Snippet = SnippetOf(mime.TextBody),
                        Received = mime.Date.LocalDateTime,
                    });
                }
                catch
                {
                    // One bad message never sinks the batch.
                }
            }

            await client.DisconnectAsync(true);
            return (true, string.Empty, messages);
        }
        catch (MailKit.Security.AuthenticationException)
        {
            try { await client.DisconnectAsync(true); } catch { }
            return (false,
                "Yahoo rejected the saved app password — it may have been revoked. " +
                "Say \"connect my yahoo\" to enter a fresh one.",
                Array.Empty<EmailMessage>());
        }
        catch (Exception ex)
        {
            try { await client.DisconnectAsync(true); } catch { }
            return (false, $"I couldn't reach Yahoo Mail: {ex.Message}", Array.Empty<EmailMessage>());
        }
    }

    public async Task<string> GetBodyAsync(EmailMessage message, int maxChars = 4000)
    {
        var cred = _vault.GetServiceCredential(VaultService.YahooCredentialName);
        if (cred == null || !UniqueId.TryParse(message.Id, out var uid))
            return string.Empty;

        var (email, appPassword) = cred.Value;
        using var client = new ImapClient();
        try
        {
            await client.ConnectAsync(ImapHost, ImapPort, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(email, appPassword);
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            var mime = await inbox.GetMessageAsync(uid);
            await client.DisconnectAsync(true);
            var body = (mime.TextBody ?? string.Empty).Trim();
            return body.Length > maxChars ? body.Substring(0, maxChars) + "…" : body;
        }
        catch
        {
            try { await client.DisconnectAsync(true); } catch { }
            return string.Empty;
        }
    }

    public void SaveAppPassword(string email, string appPassword) =>
        _vault.SaveServiceCredential(VaultService.YahooCredentialName, email.Trim(), appPassword.Trim());

    public void Disconnect() => _vault.DeleteServiceCredential(VaultService.YahooCredentialName);

    private static string SnippetOf(string? body)
    {
        body = (body ?? string.Empty).Trim();
        if (body.Length == 0)
            return string.Empty;
        var oneLine = body.Replace("\r", " ").Replace("\n", " ");
        while (oneLine.Contains("  "))
            oneLine = oneLine.Replace("  ", " ");
        return oneLine.Length > 120 ? oneLine.Substring(0, 120) + "…" : oneLine;
    }
}
