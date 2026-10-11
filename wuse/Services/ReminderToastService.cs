using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace Wuse.Services;

/// <summary>
/// Shows Windows toast notifications and runs the 60-second reminder
/// monitor: any reminder whose due time has arrived (and is not done and
/// has not been announced yet) fires a toast, exactly once per reminder.
/// Best-effort: toasts are skipped silently where the platform declines them.
/// </summary>
public sealed class ReminderToastService
{
    private static readonly Lazy<ReminderToastService> _instance = new(() => new ReminderToastService());

    public static ReminderToastService Instance => _instance.Value;

    private readonly HashSet<Guid> _notified = new();
    private DispatcherTimer? _timer;
    private ReminderStore? _store;

    private ReminderToastService()
    {
    }

    public void ShowToast(string title, string body)
    {
        try
        {
            var notification = new AppNotificationBuilder()
                .AddText(title)
                .AddText(body)
                .BuildNotification();
            AppNotificationManager.Default.Show(notification);
        }
        catch (Exception)
        {
            // Toasts are a nice-to-have; the in-app lists carry the data.
        }
    }

    /// <summary>
    /// Starts the background due-reminder check. Call once from the UI thread.
    /// </summary>
    public void StartMonitor(ReminderStore store)
    {
        if (_timer != null)
            return;
        _store = store;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
        // Run an immediate check so reminders that became due while the app
        // was closed fire right away instead of waiting a full minute.
        _ = CheckDueRemindersAsync();
    }

    private async void OnTimerTick(object? sender, object e)
    {
        await CheckDueRemindersAsync();
    }

    private async Task CheckDueRemindersAsync()
    {
        if (_store == null)
            return;
        try
        {
            var now = DateTime.Now;
            foreach (var reminder in await _store.GetAllAsync())
            {
                if (!reminder.IsDone && !_notified.Contains(reminder.Id) && reminder.DueAt <= now)
                {
                    _notified.Add(reminder.Id);
                    ShowToast("wuse Reminder", reminder.Text);
                }
            }
        }
        catch (Exception)
        {
            // The monitor must never take the app down; it simply tries again
            // on the next tick.
        }
    }
}
