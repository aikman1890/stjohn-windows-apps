using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace StablePilot.Converters;

/// <summary>
/// Colors a dispute deadline: red when overdue, orange within 5 days,
/// green when there is comfortable time left.
/// </summary>
public sealed class DaysRemainingBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int days)
        {
            if (days < 0) return new SolidColorBrush(Colors.Red);
            if (days <= 5) return new SolidColorBrush(Colors.DarkOrange);
        }
        return new SolidColorBrush(Colors.Green);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
