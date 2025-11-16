using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace InstallVibe.Converters;

/// <summary>
/// Converts null to Visible and non-null to Collapsed.
/// </summary>
public class InverseNullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
