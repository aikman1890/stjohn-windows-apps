using Windows.ApplicationModel.DataTransfer;

namespace Wuse.Services;

/// <summary>
/// Clipboard smarts: read, clear, and timed auto-clear (used by the Vault so
/// copied passwords don't linger). Clipboard content is the user's private
/// data — it's shown on request but never logged or sent anywhere.
/// </summary>
public sealed class ClipboardService
{
    private static Timer? _autoClearTimer;

    public string GetText()
    {
        try
        {
            var view = Clipboard.GetContent();
            if (view == null || !view.Contains(StandardDataFormats.Text))
                return string.Empty;
            // GetTextAsync must run on the UI thread; the caller (chat send)
            // runs there. Best-effort otherwise.
            return view.GetTextAsync().GetAwaiter().GetResult() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task CopyTextAsync(string text, bool autoClearAfterSeconds = 0)
    {
        var package = new DataPackage();
        package.SetText(text ?? string.Empty);
        Clipboard.SetContent(package);
        Clipboard.Flush();

        if (autoClearAfterSeconds > 0)
            ScheduleAutoClear(TimeSpan.FromSeconds(autoClearAfterSeconds));
        await Task.CompletedTask;
    }

    public void Clear()
    {
        try
        {
            Clipboard.Clear();
        }
        catch
        {
            // Clipboard may be locked by another app; not fatal.
        }
    }

    /// <summary>
    /// Clears the clipboard after a delay (password hygiene). If the user
    /// copies something else in the meantime, that gets cleared too — the
    /// tradeoff is documented where the feature is surfaced.
    /// </summary>
    public static void ScheduleAutoClear(TimeSpan delay)
    {
        try
        {
            _autoClearTimer?.Dispose();
            _autoClearTimer = new Timer(_ =>
            {
                try { Clipboard.Clear(); } catch { }
            }, null, delay, Timeout.InfiniteTimeSpan);
        }
        catch
        {
        }
    }
}
