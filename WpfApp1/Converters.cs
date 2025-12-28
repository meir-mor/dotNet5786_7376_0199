
using DO;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PL;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value == null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
public class CancelButtonVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BO.OrderStatus status)
        {
            return status == BO.OrderStatus.Closed
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}


public class NullToHiddenConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // If value is null, return Hidden (or Collapsed), otherwise Visible
        return value == null ? Visibility.Hidden : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Return true if the value is NOT null
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Enum e) return System.Windows.Media.Brushes.Transparent;
        return e.ToString() switch
        {
            "Car" => System.Windows.Media.Brushes.LightSkyBlue,
            "Motorcycle" => System.Windows.Media.Brushes.LightGreen,
            "Bicycle" => System.Windows.Media.Brushes.LightYellow,
            "Foot" => System.Windows.Media.Brushes.Wheat,
            "Van" => System.Windows.Media.Brushes.Plum,
            "Pending" => System.Windows.Media.Brushes.LightGray,
            "Associated" => System.Windows.Media.Brushes.LemonChiffon,
            "PickedUp" => System.Windows.Media.Brushes.LightBlue,
            "Delivered" => System.Windows.Media.Brushes.LightGreen,
            "Unscheduled" => System.Windows.Media.Brushes.Orange,
            _ => System.Windows.Media.Brushes.White
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
