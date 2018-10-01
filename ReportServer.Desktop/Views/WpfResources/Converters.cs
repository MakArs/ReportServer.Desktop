using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.ViewModels;

namespace ReportServer.Desktop.Views.WpfResources
{
    public abstract class BaseConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public abstract object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture);


        public virtual object ConvertBack(object value, Type targetType, object parameter,
                                          CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ParsingCategorySourceExtension : MarkupExtension
    {
        [Reactive] public Type EnumType { get; set; }

        public ParsingCategorySourceExtension()
        {
            EnumType = typeof(ParsingCategory);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (EnumType == null)
                throw new InvalidOperationException("The EnumType must be specified.");

            Type actualEnumType = Nullable.GetUnderlyingType(EnumType) ?? EnumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == EnumType)
                return enumValues;

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }

    public class OperModeSourceExtension : MarkupExtension
    {
        [Reactive] public Type EnumType { get; set; }

        public OperModeSourceExtension()
        {
            EnumType = typeof(OperMode);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (EnumType == null)
                throw new InvalidOperationException("The EnumType must be specified.");

            Type actualEnumType = Nullable.GetUnderlyingType(EnumType) ?? EnumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == EnumType)
                return enumValues;

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }

    public class IntMsToMinsConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            var milliseconds = (int?) value ?? 0;

            if (milliseconds == 0) return null;

            return $"{milliseconds / 60000} м {(milliseconds % 60000) / 1000.0:f0} с";
        }
    }

    public class ReportTypeToBoolConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            if (value == null) return "True";
            return value.ToString() == "Custom" ? "False" : "True";
        }
    }


    public class ReportTypeToVisiblity : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            return value.ToString() == "Custom" ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class IntToVisiblity : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            return (int?) value > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class ReportTypeToBool : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            if (value               == null) return true;
            return value.ToString() != "Custom";
        }
    }

    public class AntiReportTypeToVisiblity : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            return value.ToString() == "Custom" ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class BoolToVisiblityConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            if (value == null) return Visibility.Hidden;
            return (bool) value ? Visibility.Visible : Visibility.Hidden;
        }
    }

    public class MoreThenOneToVisibility : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            return (int?) value > 1 ? Visibility.Visible : Visibility.Hidden;
        }
    }

    public class ParsingCategoryToBool : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            return (ParsingCategory) value != ParsingCategory.All;
        }
    }

    public class IsRangeToVisibility : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            return (ParsingCategory) value == ParsingCategory.Range
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }

    public class IsNotValueToVisibility : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            return (ParsingCategory)value == ParsingCategory.Value
                ? Visibility.Hidden
                : Visibility.Visible;
        }
    }

    public class NullToVisibility : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            return value == null
                ? Visibility.Hidden
                : Visibility.Visible;
        }
    }

    public class RangeToString : BaseConverter
    {
        private readonly string[] values = new string[2];

        public override object Convert(object value, Type targetType, object parameter,
                                       CultureInfo culture)
        {
            if (value == null || !value.ToString().Contains('-'))
                return Binding.DoNothing;

            var gotValues = value.ToString().Split('-');

            if (!int.TryParse(parameter?.ToString(), out var part)
                || part < 0
                || part >= values.Length
                || part >= gotValues.Length)
                return Binding.DoNothing;

            if (string.IsNullOrEmpty(gotValues[part]) ||
                !int.TryParse(gotValues[part], out _))
                values[part] = "0";

            values[part] = gotValues[part];

            return values[part];
        }

        public override object ConvertBack(object value, Type targetType, object parameter,
                                           CultureInfo culture)
        {
            if (!int.TryParse(parameter?.ToString(), out var part)
                || part < 0
                || part >= values.Length)
                return Binding.DoNothing;

            values[part] = (string) value;

            var t=  string.Join("-", values);
            return t;
        }
    }
}
