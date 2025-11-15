using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace InstallVibe.Converters;

/// <summary>
/// Converter that converts LicenseType to color brush
/// </summary>
public class LicenseTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string licenseType)
        {
            return licenseType.ToLower() switch
            {
                "tech" => new SolidColorBrush(Color.FromArgb(255, 0, 120, 212)),      // Blue
                "pro" => new SolidColorBrush(Color.FromArgb(255, 16, 124, 16)),       // Green
                "enterprise" => new SolidColorBrush(Color.FromArgb(255, 135, 100, 184)), // Purple
                "trial" => new SolidColorBrush(Color.FromArgb(255, 255, 140, 0)),     // Orange
                _ => new SolidColorBrush(Color.FromArgb(255, 128, 128, 128))          // Gray
            };
        }

        return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
