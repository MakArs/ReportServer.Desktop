using System;
using System.Globalization;

namespace ReportServer.Desktop.Converters
{
    public class IntMsToMinsConverter:BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var milliseconds = (int?) value ?? 0;

            if (milliseconds == 0) return null;

            return $"{milliseconds / 60000} м {(milliseconds % 60000)/1000.0:f0} с";
        }
    }
}
