using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace InstallVibe.Converters;

/// <summary>
/// Converter that converts null or empty string to Collapsed, otherwise Visible
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return Visibility.Collapsed;
        }

        if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
