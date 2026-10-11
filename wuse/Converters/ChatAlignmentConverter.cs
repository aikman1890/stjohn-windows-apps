using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Wuse.Converters;

/// <summary>
/// Right-aligns user bubbles, left-aligns assistant bubbles.
/// </summary>
public sealed class ChatAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        string.Equals(value as string, "user", StringComparison.OrdinalIgnoreCase)
            ? HorizontalAlignment.Right
            : HorizontalAlignment.Left;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
