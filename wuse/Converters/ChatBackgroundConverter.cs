using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Wuse.Converters;

/// <summary>
/// Accent bubble for the user, theme-aware neutral bubble for the assistant.
/// </summary>
public sealed class ChatBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (string.Equals(value as string, "user", StringComparison.OrdinalIgnoreCase))
        {
            if (ThemeHelper.TryGetThemeColor("SystemAccentColor", out var accent))
                return new SolidColorBrush(accent);
            return new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x78, 0xD4));
        }

        if (ThemeHelper.TryGetThemeColor("ControlFillColorDefault", out var fill))
            return new SolidColorBrush(fill);

        return new SolidColorBrush(ThemeHelper.IsDarkTheme()
            ? Color.FromArgb(0xFF, 0x2D, 0x2D, 0x2D)
            : Color.FromArgb(0xFF, 0xF3, 0xF3, 0xF3));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
