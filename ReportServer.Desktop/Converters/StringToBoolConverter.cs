using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ReportServer.Desktop.Converters
{
    class StringToBoolConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "True";
            return value.ToString() == "Custom" ? "False" : "True";
        }
    }
}
