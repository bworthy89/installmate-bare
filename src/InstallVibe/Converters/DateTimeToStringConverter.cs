using Microsoft.UI.Xaml.Data;
using System;

namespace InstallVibe.Converters;

/// <summary>
/// Converter that converts DateTime to formatted string
/// </summary>
public class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            // Use parameter as format string if provided, otherwise use default
            string format = parameter as string ?? "MM/dd/yyyy";
            return dateTime.ToString(format);
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            string format = parameter as string ?? "MM/dd/yyyy";
            return dateTimeOffset.ToString(format);
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string stringValue && DateTime.TryParse(stringValue, out DateTime result))
        {
            return result;
        }

        return DateTime.MinValue;
    }
}
