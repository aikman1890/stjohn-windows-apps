using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace BizPilot.Converters;

public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null!;
    }
}
