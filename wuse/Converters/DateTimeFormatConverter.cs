using System;
using Microsoft.UI.Xaml.Data;

namespace Wuse.Converters;

/// <summary>
/// Formats a DateTime/DateTimeOffset with the format string passed as the
/// converter parameter (default "g"). Exists because {x:Bind X.ToString("g")}
/// function bindings crash the WinUI XAML compiler under dotnet CLI builds
/// (WMC9999 in BindingPathListener.CreateFunctionStep).
/// </summary>
public sealed class DateTimeFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var format = parameter as string ?? "g";
        return value switch
        {
            DateTime dt => dt.ToString(format),
            DateTimeOffset dto => dto.ToString(format),
            _ => value?.ToString() ?? string.Empty,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
