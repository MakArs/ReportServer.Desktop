using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using ReactiveUI.Fody.Helpers;

namespace ReportServer.Desktop.Views.WpfResources
{
    public abstract class BaseConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);


        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumBindingSourceExtension : MarkupExtension
    {
        [Reactive] public Type EnumType { get; set; }

        public EnumBindingSourceExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (EnumType == null)
                throw new InvalidOperationException("The EnumType must be specified.");

            Type  actualEnumType = Nullable.GetUnderlyingType(EnumType) ?? EnumType;
            Array enumValues     = Enum.GetValues(actualEnumType);

            if (actualEnumType == EnumType)
                return enumValues;

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }

    public class IntMsToMinsConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var milliseconds = (int?)value ?? 0;

            if (milliseconds == 0) return null;

            return $"{milliseconds / 60000} м {(milliseconds % 60000) / 1000.0:f0} с";
        }
    }

    public class ReportTypeToBoolConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "True";
            return value.ToString() == "Custom" ? "False" : "True";
        }
    }

    public class ReportTypeToVisiblity : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            return value.ToString() == "Custom" ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class AntiReportTypeToVisiblity : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            return value.ToString() == "Custom" ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class NullToVisiblityConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Hidden : Visibility.Visible;
        }
    }

    public class BoolToVisiblityConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Hidden;
            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
