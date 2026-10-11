using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Identity.Client;

namespace Wuse.Services;

/// <summary>
/// Outlook / Office 365 calendar via Microsoft Graph REST (v1.0), across
/// every connected Microsoft account.
/// </summary>
public sealed class OutlookCalendarClient
{
    private readonly MicrosoftAuthService _auth;
    private static readonly HttpClient Http = new();

    public OutlookCalendarClient(MicrosoftAuthService auth) => _auth = auth;

    public async Task<(bool Ok, string Error, IReadOnlyList<CalendarEvent> Events)> GetTodayAsync()
    {
        if (!_auth.IsConfigured)
            return (false, MicrosoftAuthService.MicrosoftSetupMessage, Array.Empty<CalendarEvent>());

        var accounts = await _auth.GetAccountsAsync();
        if (accounts.Count == 0)
            return (false,
                "No Microsoft account is connected yet. Say \"connect my outlook\" and I'll open the sign-in page.",
                Array.Empty<CalendarEvent>());

        var all = new List<CalendarEvent>();
        var errors = new List<string>();
        foreach (var account in accounts)
        {
            var token = await _auth.GetTokenSilentAsync(account);
            if (token == null)
            {
                errors.Add($"{MicrosoftAuthService.AccountLabel(account)} needs you to sign in again.");
                continue;
            }
            var (ok, err, events) = await GetTodayForAccountAsync(token, account);
            if (!ok)
                errors.Add(err);
            else
                all.AddRange(events);
        }

        all = all.OrderBy(e => e.Start).ToList();
        if (all.Count == 0 && errors.Count > 0)
            return (false, string.Join(" ", errors), all);
        return (true, string.Empty, all);
    }

    public async Task<(bool Ok, string Error, string AccountUsed)> CreateEventAsync(string title, DateTime start, DateTime end)
    {
        if (!_auth.IsConfigured)
            return (false, MicrosoftAuthService.MicrosoftSetupMessage, string.Empty);

        var accounts = await _auth.GetAccountsAsync();
        if (accounts.Count == 0)
            return (false, "No Microsoft account is connected yet.", string.Empty);

        // Use the first account with a valid token; the confirmation prompt
        // names it so there are no surprises.
        foreach (var account in accounts)
        {
            var token = await _auth.GetTokenSilentAsync(account);
            if (token == null)
                continue;
            var label = MicrosoftAuthService.AccountLabel(account);
            var payload = new
            {
                subject = title,
                start = new { dateTime = start.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = TimeZoneInfo.Local.Id },
                end = new { dateTime = end.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = TimeZoneInfo.Local.Id },
            };
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/me/events")
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var resp = await Http.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                    return (true, string.Empty, label);
            }
            catch
            {
                // Try the next account.
            }
        }
        return (false, "I couldn't save that event to any connected Outlook calendar.", string.Empty);
    }

    private static async Task<(bool Ok, string Error, IReadOnlyList<CalendarEvent> Events)> GetTodayForAccountAsync(
        string token, IAccount account)
    {
        try
        {
            var label = MicrosoftAuthService.AccountLabel(account);
            var start = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:ss");
            var end = DateTime.Today.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss");
            var url = "https://graph.microsoft.com/v1.0/me/calendarview" +
                      $"?startdatetime={start}&enddatetime={end}" +
                      "&$select=subject,start,end,location" +
                      "&$orderby=start/dateTime";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var resp = await Http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return (false, $"Calendar ({label}) returned {(int)resp.StatusCode}.", Array.Empty<CalendarEvent>());

            var events = new List<CalendarEvent>();
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            foreach (var item in doc.RootElement.GetProperty("value").EnumerateArray())
            {
                var title = item.TryGetProperty("subject", out var s) ? s.GetString() ?? "(no title)" : "(no title)";
                var location = "";
                if (item.TryGetProperty("location", out var loc) &&
                    loc.TryGetProperty("displayName", out var dn))
                    location = dn.GetString() ?? "";
                events.Add(new CalendarEvent
                {
                    Provider = "Outlook",
                    AccountLabel = label,
                    Title = string.IsNullOrWhiteSpace(title) ? "(no title)" : title,
                    Start = ParseGraphDateTime(item, "start"),
                    End = ParseGraphDateTime(item, "end"),
                    Location = location,
                });
            }
            return (true, string.Empty, events);
        }
        catch (Exception ex)
        {
            return (false, $"I couldn't reach that Outlook calendar: {ex.Message}", Array.Empty<CalendarEvent>());
        }
    }

    private static DateTime ParseGraphDateTime(JsonElement item, string name)
    {
        try
        {
            var dt = item.GetProperty(name).GetProperty("dateTime").GetString() ?? "";
            if (DateTime.TryParse(dt, out var parsed))
                return parsed;
        }
        catch
        {
        }
        return DateTime.Today;
    }
}
