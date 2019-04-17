using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using ReportServer.Desktop.Entities;

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

    public abstract class BaseMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

        public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TaskIdAndRoleToVisMultiConverter : BaseMultiConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (ServiceUserRole) values[0] == ServiceUserRole.Editor && (int?) values[1] > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            catch //todo: remove (second and other windows open with bug-2 arrays creating)
            {
                return Visibility.Collapsed;
            }
        }
    }

    public class InstanceStateAndRoleToVisMultiConverter : BaseMultiConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ServiceUserRole) values[0] == ServiceUserRole.Editor ||
                    (ServiceUserRole) values[0] == ServiceUserRole.StopRunner) &&
                   (InstanceState) values[1] == InstanceState.InProcess
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    public class IsDefaultAndTextToTextBoxMultiConverter : BaseMultiConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool isChecked)
                return isChecked ? "Default path" :
                       values[1]?.ToString();

            if (values[1] is bool isChecked2)
                return isChecked2 ? "Default path" :
                    values[0]?.ToString();
            return null;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (targetTypes[0].GenericTypeArguments.First()==typeof(bool))
            {
                return value?.ToString() == "Default path" ? new object[] { true ,"Default path" } : new object[] { false, value?.ToString() };
            }
                return value?.ToString() == "Default path" ? new object[] { "Default path" ,true} : new object[]{ value?.ToString(), false};
        }
    }

   public class InverseBoolConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class BoolToDefaultTextConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Default path" : Binding.DoNothing;
        }
    }

    public class DefaultTextToBoolConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() =="Default path";
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
                ? Visibility.Collapsed
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
