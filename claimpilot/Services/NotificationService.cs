using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace ClaimPilot.Services;

/// <summary>
/// Local toast reminders for user deadlines. Everything is on-device: a
/// DispatcherQueue timer checks SQLite deadlines every 15 minutes and shows a
/// toast for anything due within 24 hours or overdue (one toast per deadline
/// per day). All failures are swallowed — reminders are best-effort.
/// </summary>
public static class NotificationService
{
    private static DispatcherQueueTimer? _timer;
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;

        try
        {
            AppNotificationManager.Default.Register();
        }
        catch
        {
            // Registration is best-effort; Show is separately guarded.
        }

        try
        {
            var queue = DispatcherQueue.GetForCurrentThread();
            _timer = queue.CreateTimer();
            _timer.Interval = TimeSpan.FromMinutes(15);
            _timer.Tick += async (_, _) => await CheckDeadlinesAsync();
            _timer.Start();

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
                await CheckDeadlinesAsync();
            });
        }
        catch
        {
            // Timer init failure must not crash startup.
        }
    }

    public static void ShowToast(string title, string body)
    {
        try
        {
            var notification = new AppNotificationBuilder()
                .AddText(title)
                .AddText(body)
                .BuildNotification();
            AppNotificationManager.Default.Show(notification);
        }
        catch
        {
            // Toast failure is non-fatal.
        }
    }

    private static async Task CheckDeadlinesAsync()
    {
        try
        {
            var todayKey = DateTime.Today.ToString("yyyyMMdd");
            var deadlines = await DatabaseService.Instance.GetDeadlinesAsync();
            foreach (var deadline in deadlines)
            {
                if (deadline.IsDone)
                {
                    continue;
                }

                var daysUntil = (deadline.DueDate.Date - DateTime.Today).TotalDays;
                if (daysUntil > 1)
                {
                    continue;
                }

                if (deadline.NotifiedKey == todayKey)
                {
                    continue;
                }

                var when = daysUntil < 0
                    ? $"was due {deadline.DueDate:MMMM d, yyyy} (overdue)"
                    : daysUntil < 1 ? "is due today" : "is due tomorrow";

                ShowToast("ClaimPilot deadline reminder", $"\"{deadline.Title}\" {when}.");
                deadline.NotifiedKey = todayKey;
                await DatabaseService.Instance.UpdateDeadlineAsync(deadline);
            }
        }
        catch
        {
            // Reminder sweep failure is non-fatal.
        }
    }
}
