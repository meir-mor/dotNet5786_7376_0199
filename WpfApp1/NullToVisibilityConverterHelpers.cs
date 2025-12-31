using System.Globalization;
using System.Windows;

namespace PL
{
    internal static class NullToVisibilityConverterHelpers
    {
        public static object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible ? parameter : null;
            }

            return null;
        }
    }
}