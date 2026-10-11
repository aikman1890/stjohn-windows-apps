using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Wuse.Converters;

/// <summary>
/// White text on the accent user bubble, theme text color on assistant bubbles.
/// </summary>
public sealed class ChatForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (string.Equals(value as string, "user", StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Colors.White);

        if (ThemeHelper.TryGetThemeColor("TextFillColorPrimary", out var text))
            return new SolidColorBrush(text);

        return new SolidColorBrush(ThemeHelper.IsDarkTheme() ? Colors.White : Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
