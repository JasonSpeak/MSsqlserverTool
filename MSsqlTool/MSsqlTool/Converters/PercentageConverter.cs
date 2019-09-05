using System;
using System.Windows.Data;

namespace MSsqlTool.Converters
{
    internal class PercentageConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,System.Globalization.CultureInfo culture)
        {
            if (parameter != null && value != null && 
                double.TryParse(value.ToString(), out var convertValue) && 
                double.TryParse(parameter.ToString(), out var convertParameter))
            {
                return convertValue * convertParameter;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
