using System;
using System.Windows;
using System.Windows.Data;
using NLog;

namespace MSsqlTool.ViewModel
{
    internal class BoolToVisibleConverter:IValueConverter 
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var isVisible = System.Convert.ToBoolean(value);
                return isVisible ? Visibility.Visible : Visibility.Hidden;
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
