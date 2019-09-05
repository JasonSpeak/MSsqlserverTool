using System;
using System.Windows;
using System.Windows.Data;

namespace MSsqlTool.Converters
{
    internal class BoolToVisibilityConverter:IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && bool.TryParse(value.ToString(), out var isVisible))
            {
                return isVisible ? Visibility.Visible : Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
