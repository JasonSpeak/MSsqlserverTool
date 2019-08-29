using System;
using System.Windows.Data;
using NLog;

namespace MSsqlTool.ViewModel
{
    internal class PercentageConverter:IValueConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public object Convert(object value, Type targetType, object parameter,System.Globalization.CultureInfo culture)
        {
            try
            {
                var convertValue = System.Convert.ToDouble(value);
                var convertParameter = System.Convert.ToDouble(parameter);
                return convertValue * convertParameter;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
