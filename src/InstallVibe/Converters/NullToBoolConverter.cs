using Microsoft.UI.Xaml.Data;
using System;

namespace InstallVibe.Converters;

/// <summary>
/// Converter that converts null or empty string to false, otherwise true
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return false;
        }

        if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
        {
            return false;
        }

        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
