using Microsoft.UI.Xaml.Data;

namespace InstallVibe.Converters;

/// <summary>
/// Converts a boolean value to different glyph icons.
/// Parameter format: "TrueGlyph|FalseGlyph" (e.g., "&#xE840;|&#xE842;")
/// </summary>
public class BoolToGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool boolValue)
            return "&#xE8BA;"; // Default icon

        if (parameter is not string paramString || !paramString.Contains('|'))
            return "&#xE8BA;";

        var parts = paramString.Split('|');
        if (parts.Length != 2)
            return "&#xE8BA;";

        return boolValue ? parts[0] : parts[1];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
