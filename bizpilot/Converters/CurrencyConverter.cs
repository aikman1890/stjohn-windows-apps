using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace BizPilot.Converters;

public sealed class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            double d => d.ToString("C", CultureInfo.CurrentCulture),
            float f => ((double)f).ToString("C", CultureInfo.CurrentCulture),
            decimal m => m.ToString("C", CultureInfo.CurrentCulture),
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string text
            && double.TryParse(text, NumberStyles.Currency, CultureInfo.CurrentCulture, out var parsed))
        {
            return parsed;
        }

        return 0d;
    }
}
