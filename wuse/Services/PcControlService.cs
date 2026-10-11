using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.System;

namespace Wuse.Services;

/// <summary>
/// PC control: launching apps by friendly name (extended table + Start-menu
/// shortcut search), volume keys, workstation lock, and special folders.
/// Refuses destructive actions — those live in SafetyGuard and never execute.
/// </summary>
public sealed class PcControlService
{
    // ---- Win32 for volume keys + lock ----
    private const int VK_VOLUME_MUTE = 0xAD;
    private const int VK_VOLUME_DOWN = 0xAE;
    private const int VK_VOLUME_UP = 0xAF;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool LockWorkStation();

    // Well-known executables / URIs by friendly name. Anything not here falls
    // through to the Start-menu shortcut search.
    private static readonly Dictionary<string, string> KnownApps = new(StringComparer.OrdinalIgnoreCase)
    {
        ["spotify"] = "spotify.exe",
        ["chrome"] = "chrome.exe",
        ["google chrome"] = "chrome.exe",
        ["firefox"] = "firefox.exe",
        ["vscode"] = "code.exe",
        ["vs code"] = "code.exe",
        ["visual studio code"] = "code.exe",
        ["discord"] = "discord.exe",
        ["steam"] = "steam.exe",
        ["zoom"] = "zoom.exe",
        ["teams"] = "teams.exe",
        ["word"] = "winword.exe",
        ["excel"] = "excel.exe",
        ["powerpoint"] = "powerpnt.exe",
        ["onenote"] = "onenote.exe",
        ["vlc"] = "vlc.exe",
        ["telegram"] = "telegram.exe",
        ["whatsapp"] = "whatsapp.exe",
        ["snipping tool"] = "ms-screenclip:",
        ["sticky notes"] = "ms-stickynotes:",
        ["photos"] = "ms-photos:",
        ["camera"] = "microsoft.windows.camera:",
        ["media player"] = "mswindowsmusic:",
    };

    /// <summary>
    /// Launches an app by friendly name: known exe/URI table first, then the
    /// user's Start menu. Returns a user-facing result string.
    /// </summary>
    public async Task<string> OpenAppAsync(string name)
    {
        var key = (name ?? string.Empty).Trim().Trim('"', '\'');
        if (key.Length == 0)
            return "Tell me which app to open — for example, \"open Spotify\".";

        try
        {
            // 1. Known apps.
            if (KnownApps.TryGetValue(key, out var target))
            {
                if (target.EndsWith(":", StringComparison.Ordinal))
                {
                    var opened = await Launcher.LaunchUriAsync(new Uri(target));
                    return opened ? $"Opening {key}…" : $"The system declined to open {key}.";
                }
                Process.Start(target);
                return $"Opening {key}…";
            }

            // 2. Start-menu shortcut search.
            var matches = FindStartMenuShortcuts(key);
            if (matches.Count == 1)
            {
                Process.Start(new ProcessStartInfo(matches[0]) { UseShellExecute = true });
                return $"Opening {Path.GetFileNameWithoutExtension(matches[0])}…";
            }
            if (matches.Count > 1)
            {
                var names = matches.Take(5).Select(m => Path.GetFileNameWithoutExtension(m));
                return $"I found a few matches for \"{key}\": {string.Join(", ", names)}. " +
                       "Which one did you mean?";
            }

            // 3. Last resort: try it as an exe name directly.
            try
            {
                Process.Start(key.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? key : key + ".exe");
                return $"Opening {key}…";
            }
            catch
            {
                return $"I couldn't find an app called \"{key}\" on this PC. " +
                       "Try the exact name as it appears in your Start menu.";
            }
        }
        catch (Exception ex)
        {
            return $"I couldn't open \"{key}\": {ex.Message}";
        }
    }

    /// <summary>
    /// Adjusts the system volume via media-key injection. Steps of ~2% each.
    /// </summary>
    public string AdjustVolume(string direction)
    {
        var dir = (direction ?? string.Empty).Trim().ToLowerInvariant();
        try
        {
            switch (dir)
            {
                case "up":
                    PressVolumeKey(VK_VOLUME_UP, 4);
                    return "Turned it up a notch.";
                case "down":
                    PressVolumeKey(VK_VOLUME_DOWN, 4);
                    return "Turned it down a notch.";
                case "mute":
                    PressVolumeKey(VK_VOLUME_MUTE, 1);
                    return "Muted.";
                case "unmute":
                    // Unmute = toggle; only press if we can't know state, so say what we did.
                    PressVolumeKey(VK_VOLUME_MUTE, 1);
                    return "Toggled mute — if it was muted, you're unmuted now.";
                default:
                    return "Say \"turn it up\", \"turn it down\", \"mute\", or \"unmute\".";
            }
        }
        catch (Exception ex)
        {
            return $"I couldn't change the volume: {ex.Message}";
        }
    }

    public string LockPc()
    {
        try
        {
            return LockWorkStation()
                ? "Locking your PC now — see you when you're back!"
                : "I couldn't lock the PC just now.";
        }
        catch (Exception ex)
        {
            return $"I couldn't lock the PC: {ex.Message}";
        }
    }

    public async Task<string> OpenSpecialFolderAsync(string which)
    {
        var key = (which ?? string.Empty).Trim().ToLowerInvariant();
        string? path = key switch
        {
            "documents" or "my documents" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "downloads" or "my downloads" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            "desktop" or "my desktop" => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "pictures" or "my pictures" => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "music" or "my music" => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            "videos" or "my videos" => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            _ => null,
        };
        if (path == null)
            return "I can open your Documents, Downloads, Desktop, Pictures, Music, or Videos folders.";
        if (!Directory.Exists(path))
            return $"I couldn't find that folder on this PC.";

        try
        {
            var opened = await Launcher.LaunchFolderPathAsync(path);
            return opened ? $"Opening {key}…" : "The system declined to open that folder.";
        }
        catch (Exception ex)
        {
            return $"I couldn't open that folder: {ex.Message}";
        }
    }

    private static void PressVolumeKey(int vk, int times)
    {
        for (int i = 0; i < times; i++)
        {
            var down = new INPUT
            {
                type = 1,
                u = new InputUnion { ki = new KEYBDINPUT { wVk = (ushort)vk } },
            };
            var up = new INPUT
            {
                type = 1,
                u = new InputUnion { ki = new KEYBDINPUT { wVk = (ushort)vk, dwFlags = KEYEVENTF_KEYUP } },
            };
            SendInput(1, new[] { down }, Marshal.SizeOf<INPUT>());
            SendInput(1, new[] { up }, Marshal.SizeOf<INPUT>());
        }
    }

    private static List<string> FindStartMenuShortcuts(string query)
    {
        var results = new List<string>();
        var roots = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"),
        };
        foreach (var root in roots)
        {
            if (!Directory.Exists(root))
                continue;
            try
            {
                // Root level + one level of subfolders: fast, covers ~everything.
                foreach (var file in Directory.EnumerateFiles(root, "*.lnk", SearchOption.TopDirectoryOnly))
                    if (Path.GetFileNameWithoutExtension(file).Contains(query, StringComparison.OrdinalIgnoreCase))
                        results.Add(file);
                foreach (var dir in Directory.EnumerateDirectories(root))
                {
                    foreach (var file in Directory.EnumerateFiles(dir, "*.lnk", SearchOption.TopDirectoryOnly))
                        if (Path.GetFileNameWithoutExtension(file).Contains(query, StringComparison.OrdinalIgnoreCase))
                            results.Add(file);
                }
            }
            catch
            {
                // Inaccessible corners of the Start menu are skipped silently.
            }
        }
        return results.Distinct(StringComparer.OrdinalIgnoreCase).Take(8).ToList();
    }
}
