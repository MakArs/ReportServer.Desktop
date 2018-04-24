using System;
using System.Globalization;
using System.Windows;

namespace ReportServer.Desktop.Converters
{
    class NullToVisiblityConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Hidden : Visibility.Visible;
        }
    }

    class BoolToVisiblityConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Hidden;
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
