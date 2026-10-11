using System;
using Microsoft.UI.Xaml.Data;

namespace BizPilot.Converters;

public sealed class DateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            DateTimeOffset dto => dto.ToString("d"),
            DateTime dt => dt.ToString("d"),
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string text && DateTimeOffset.TryParse(text, out var parsed))
        {
            return parsed;
        }

        return DateTimeOffset.Now;
    }
}
