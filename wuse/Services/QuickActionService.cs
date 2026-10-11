using System.Diagnostics;
using System.IO;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System;

namespace Wuse.Services;

/// <summary>
/// Real on-device actions that work without any AI: opening apps, folders,
/// web search, and machine info. Every method returns a user-facing result string.
/// </summary>
public sealed class QuickActionService
{
    private static readonly Dictionary<string, Func<Task<string>>> AppLaunchers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["calculator"] = () => LaunchUriAsync("calculator://", "Opening Calculator…"),
        ["calc"] = () => LaunchUriAsync("calculator://", "Opening Calculator…"),
        ["store"] = () => LaunchUriAsync("ms-windows-store://", "Opening Microsoft Store…"),
        ["microsoft store"] = () => LaunchUriAsync("ms-windows-store://", "Opening Microsoft Store…"),
        ["settings"] = () => LaunchUriAsync("ms-settings:", "Opening Settings…"),
        ["browser"] = () => LaunchUriAsync("https://www.bing.com", "Opening your browser…"),
        ["web"] = () => LaunchUriAsync("https://www.bing.com", "Opening your browser…"),
        ["edge"] = () => LaunchUriAsync("https://www.bing.com", "Opening your browser…"),
        ["notepad"] = () => LaunchExeAsync("notepad.exe", "Opening Notepad…"),
        ["paint"] = () => LaunchExeAsync("mspaint.exe", "Opening Paint…"),
        ["terminal"] = () => LaunchTerminalAsync(),
        ["windows terminal"] = () => LaunchTerminalAsync(),
        ["files"] = () => LaunchExeAsync("explorer.exe", "Opening File Explorer…"),
        ["file explorer"] = () => LaunchExeAsync("explorer.exe", "Opening File Explorer…"),
        ["explorer"] = () => LaunchExeAsync("explorer.exe", "Opening File Explorer…"),
    };

    public Task<string> OpenAppAsync(string name)
    {
        var key = (name ?? string.Empty).Trim();
        if (key.Length == 0)
            return Task.FromResult("Tell me which app to open — for example, \"open calculator\".");

        if (AppLaunchers.TryGetValue(key, out var launch))
        {
            try
            {
                return launch();
            }
            catch (Exception ex)
            {
                return Task.FromResult($"I couldn't open \"{key}\": {ex.Message}");
            }
        }

        return Task.FromResult(
            $"I don't know how to open \"{key}\" yet. " +
            "Try: calculator, notepad, paint, settings, store, terminal, browser, or files.");
    }

    public async Task<string> OpenFolderAsync(string path)
    {
        var folder = (path ?? string.Empty).Trim().Trim('"');
        if (folder.Length == 0)
            return "Give me a folder path to open — for example, \"open folder C:\\Users\".";
        if (!Directory.Exists(folder))
            return $"I couldn't find the folder \"{folder}\".";

        try
        {
            var opened = await Launcher.LaunchFolderPathAsync(folder);
            return opened ? $"Opening {folder}…" : $"The system declined to open \"{folder}\".";
        }
        catch (Exception ex)
        {
            return $"I couldn't open that folder: {ex.Message}";
        }
    }

    public async Task<string> WebSearchAsync(string query)
    {
        var q = (query ?? string.Empty).Trim();
        if (q.Length == 0)
            return "What would you like me to search for?";

        try
        {
            var url = "https://www.bing.com/search?q=" + Uri.EscapeDataString(q);
            var opened = await Launcher.LaunchUriAsync(new Uri(url));
            return opened ? $"Searching the web for \"{q}\"…" : "The system declined to open the browser.";
        }
        catch (Exception ex)
        {
            return $"I couldn't start that search: {ex.Message}";
        }
    }

    public Task<string> MachineInfoAsync()
    {
        try
        {
            var device = new EasClientDeviceInformation();
            var info = new StringBuilder();
            info.AppendLine($"Device name: {device.FriendlyName}");
            info.AppendLine($"Manufacturer: {device.SystemManufacturer}");
            info.AppendLine($"Model: {device.SystemProductName}");
            info.AppendLine($"Operating system: {device.OperatingSystem}");
            info.AppendLine($"User: {Environment.UserName}");
            info.AppendLine($"Processors: {Environment.ProcessorCount}");
            info.AppendLine($"64-bit OS: {(Environment.Is64BitOperatingSystem ? "yes" : "no")}");
            return Task.FromResult(info.ToString().TrimEnd());
        }
        catch (Exception ex)
        {
            return Task.FromResult($"I couldn't read this PC's info: {ex.Message}");
        }
    }

    public async Task<string> CopyMachineInfoAsync()
    {
        var info = await MachineInfoAsync();
        try
        {
            var package = new DataPackage();
            package.SetText(info);
            Clipboard.SetContent(package);
            return "Machine info copied to your clipboard.";
        }
        catch (Exception ex)
        {
            return $"I couldn't copy that: {ex.Message}";
        }
    }

    private static async Task<string> LaunchUriAsync(string uri, string okMessage)
    {
        try
        {
            var opened = await Launcher.LaunchUriAsync(new Uri(uri));
            return opened ? okMessage : $"The system declined to open {uri}.";
        }
        catch (Exception ex)
        {
            return $"I couldn't open that: {ex.Message}";
        }
    }

    private static Task<string> LaunchExeAsync(string exe, string okMessage)
    {
        try
        {
            Process.Start(exe);
            return Task.FromResult(okMessage);
        }
        catch (Exception ex)
        {
            return Task.FromResult($"I couldn't start {exe}: {ex.Message}");
        }
    }

    private static Task<string> LaunchTerminalAsync()
    {
        try
        {
            Process.Start("wt.exe");
            return Task.FromResult("Opening Terminal…");
        }
        catch
        {
            return LaunchExeAsync("cmd.exe", "Opening Command Prompt…");
        }
    }
}
