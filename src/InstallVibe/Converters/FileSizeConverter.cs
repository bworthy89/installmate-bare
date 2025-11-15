using Microsoft.UI.Xaml.Data;
using System;

namespace InstallVibe.Converters;

/// <summary>
/// Converter that converts file size in bytes to human-readable string (KB, MB, GB)
/// </summary>
public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long bytes)
        {
            return FormatFileSize(bytes);
        }

        if (value is int intBytes)
        {
            return FormatFileSize(intBytes);
        }

        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
