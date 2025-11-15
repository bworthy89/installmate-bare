using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace InstallVibe.Converters;

/// <summary>
/// Converts a boolean value to different colors.
/// Parameter format: "TrueColor|FalseColor" (e.g., "Accent|Secondary")
/// Supports: Accent, Secondary, Tertiary, or hex colors like #FF0000
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool boolValue)
            return new SolidColorBrush(Colors.Gray);

        if (parameter is not string paramString || !paramString.Contains('|'))
            return new SolidColorBrush(Colors.Gray);

        var parts = paramString.Split('|');
        if (parts.Length != 2)
            return new SolidColorBrush(Colors.Gray);

        var colorName = boolValue ? parts[0] : parts[1];

        return colorName.ToLower() switch
        {
            "accent" => new SolidColorBrush(Color.FromArgb(255, 0, 120, 212)), // Windows accent blue
            "secondary" => new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)), // Gray
            "tertiary" => new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)), // Light gray
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
