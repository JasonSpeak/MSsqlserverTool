using System;
using System.Windows;
using System.Windows.Data;

namespace MSsqlTool.ViewModel
{
    internal class BoolToVisibleConverter:IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isVisible = System.Convert.ToBoolean(value);
            if (isVisible)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
