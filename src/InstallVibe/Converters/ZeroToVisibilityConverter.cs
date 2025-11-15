using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace InstallVibe.Converters;

/// <summary>
/// Converts a numeric value to a Visibility value.
/// Zero returns Collapsed, non-zero returns Visible.
/// </summary>
public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue)
        {
            return intValue == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        if (value is double doubleValue)
        {
            return doubleValue == 0.0 ? Visibility.Collapsed : Visibility.Visible;
        }

        if (value is long longValue)
        {
            return longValue == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
