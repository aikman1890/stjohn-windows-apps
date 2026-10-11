using Microsoft.UI;
using Microsoft.UI.Xaml;

namespace Wuse.Converters;

/// <summary>
/// Shared theme helpers for the chat converters. Every resource lookup has a
/// hardcoded fallback so bubbles always render, even if a theme key is missing.
/// </summary>
internal static class ThemeHelper
{
    internal static bool TryGetThemeColor(string key, out Microsoft.UI.Color color)
    {
        color = default;
        try
        {
            if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Microsoft.UI.Color c)
            {
                color = c;
                return true;
            }
        }
        catch { }
        return false;
    }

    internal static bool IsDarkTheme()
    {
        try
        {
            return Application.Current?.RequestedTheme == ApplicationTheme.Dark;
        }
        catch
        {
            return false;
        }
    }
}
