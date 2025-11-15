using Microsoft.UI.Xaml.Data;
using System;

namespace InstallVibe.Converters;

/// <summary>
/// Converter that converts media type to icon glyph
/// </summary>
public class MediaTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string mediaType)
        {
            return mediaType.ToLower() switch
            {
                "image" => "\uEB9F",      // Picture icon
                "video" => "\uE714",      // Video icon
                "pdf" => "\uE8A5",        // PDF icon
                "document" => "\uE8A5",   // Document icon
                _ => "\uE8A5"             // Generic document icon
            };
        }

        return "\uE8A5"; // Default document icon
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
