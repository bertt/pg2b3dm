using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace pg2b3dm.ui.Converters;

public class BoolToRunTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isRunning)
        {
            return isRunning ? "?? Processing..." : "? Run pg2b3dm";
        }
        return "? Run pg2b3dm";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
