using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace InstallVibe.Converters;

/// <summary>
/// Converts a string value to a Visibility value.
/// Empty or null strings return Collapsed.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string str)
        {
            return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
