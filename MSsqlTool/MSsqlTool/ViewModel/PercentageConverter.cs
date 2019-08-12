using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MSsqlTool.ViewModel
{
    internal class PercentageConverter:IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter,System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targeType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
