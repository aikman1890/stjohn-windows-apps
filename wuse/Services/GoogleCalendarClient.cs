using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace Wuse.Services;

/// <summary>
/// Google Calendar via Google's .NET client library: today's events + create.
/// </summary>
public sealed class GoogleCalendarClient
{
    private readonly GoogleAuthService _auth;

    public GoogleCalendarClient(GoogleAuthService auth) => _auth = auth;

    public async Task<(bool Ok, string Error, IReadOnlyList<CalendarEvent> Events)> GetTodayAsync()
    {
        var (ok, error, cred) = await _auth.EnsureCredentialAsync();
        if (!ok || cred == null)
            return (false, error, Array.Empty<CalendarEvent>());

        try
        {
            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = "wuse",
            });
            var req = service.Events.List("primary");
            req.TimeMinDateTimeOffset = DateTime.Today;
            req.TimeMaxDateTimeOffset = DateTime.Today.AddDays(1);
            req.SingleEvents = true;
            req.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            var result = await req.ExecuteAsync();

            var events = new List<CalendarEvent>();
            if (result.Items != null)
            {
                foreach (var e in result.Items)
                {
                    var start = e.Start?.DateTimeDateTimeOffset?.LocalDateTime
                                ?? e.Start?.DateDateTimeOffset?.LocalDateTime
                                ?? DateTime.Today;
                    var end = e.End?.DateTimeDateTimeOffset?.LocalDateTime
                              ?? e.End?.DateDateTimeOffset?.LocalDateTime
                              ?? start.AddHours(1);
                    events.Add(new CalendarEvent
                    {
                        Provider = "Google Calendar",
                        AccountLabel = "Google",
                        Title = string.IsNullOrWhiteSpace(e.Summary) ? "(no title)" : e.Summary,
                        Start = start,
                        End = end,
                        Location = e.Location ?? string.Empty,
                    });
                }
            }
            return (true, string.Empty, events);
        }
        catch (Exception ex)
        {
            return (false, $"I couldn't reach Google Calendar: {ex.Message}", Array.Empty<CalendarEvent>());
        }
    }

    public async Task<(bool Ok, string Error)> CreateEventAsync(string title, DateTime start, DateTime end)
    {
        var (ok, error, cred) = await _auth.EnsureCredentialAsync();
        if (!ok || cred == null)
            return (false, error);
        try
        {
            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = "wuse",
            });
            var ev = new Event
            {
                Summary = title,
                Start = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(start) },
                End = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(end) },
            };
            await service.Events.Insert(ev, "primary").ExecuteAsync();
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, $"Google Calendar wouldn't save it: {ex.Message}");
        }
    }
}
