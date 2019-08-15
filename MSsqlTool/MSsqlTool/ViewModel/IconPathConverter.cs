using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MSsqlTool.ViewModel
{
    internal class IconPathConverter:IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, System.Globalization.CultureInfo culture)
        {
            string level = System.Convert.ToString(value);
            if (level == "tables")
            {
                return @"..\Icons\table.png";
            }
            else
            {
                return @"..\Icons\database.png";
            }

        }

        public object ConvertBack(object value, Type targeType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
