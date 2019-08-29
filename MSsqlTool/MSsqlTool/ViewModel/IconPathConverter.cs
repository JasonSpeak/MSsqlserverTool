using System;
using System.Windows.Data;
using NLog;

namespace MSsqlTool.ViewModel
{
    internal class IconPathConverter:IValueConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var level = System.Convert.ToString(value);
                return level == "tables" ? @"..\Icons\table.png" : @"..\Icons\database.png";
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
