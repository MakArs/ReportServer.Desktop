using System;
using System.Globalization;
using System.Windows;

namespace ReportServer.Desktop.Converters
{
    class TaskTypeToBoolConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "True";
            return value.ToString() == "Custom" ? "False" : "True";
        }
    }
    class TaskTypeToVisiblity : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            return value.ToString() == "Custom" ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
