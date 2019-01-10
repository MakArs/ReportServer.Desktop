using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autofac;
using DynamicData;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using Ui.Wpf.Common;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using ItemCollection = Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection;

namespace ReportServer.Desktop.Views.WpfResources
{

    public class RecepGroupsSource : IItemsSource
    {
        private SourceList<ApiRecepientGroup> groups  =
            new SourceList<ApiRecepientGroup>();

        public static IContainer Container;

        public ItemCollection GetValues()
        {
            ItemCollection coll = new ItemCollection();

            var cach = Container.Resolve<ICachedService>();
            lock (this)
                groups.ClearAndAddRange(cach.RecepientGroups.Items);

            coll.Add(0,"None");

            foreach (var rgr in groups.Items)
                coll.Add(rgr.Id, rgr.Name);

            return coll;
        }
    }

    public class DelimitersSource : IItemsSource
    {
        private SourceList<Delimiter> delimiters  =
            new SourceList<Delimiter>();

        private class Delimiter
        {
            public string Value;
            public string Name;
        }

        public ItemCollection GetValues()
        {
            ItemCollection coll = new ItemCollection();

            var delimitersList = new List<Delimiter>
            {
                new Delimiter {Value = ";", Name = "Semicolon"},
                new Delimiter {Value = ",", Name = "Comma"},
                // new Delimiter {Value = " ", Name = "Space"},
                new Delimiter {Value = "\\t", Name = "Tabulation"},
                new Delimiter {Value = "|", Name = "Pipe"},
                new Delimiter {Value = ".", Name = "Dot"},
                new Delimiter {Value = "\\r\\n", Name = "New line"},
            };

            delimiters.ClearAndAddRange(delimitersList);

            foreach (var delim in delimitersList)
                coll.Add(delim.Value,delim.Name);

            return coll;
        }
    }

    public class TelegramChannelsSource : IItemsSource
    {
        private SourceList<ApiTelegramChannel> channels  =
            new SourceList<ApiTelegramChannel>();

        public static IContainer Container;

        public ItemCollection GetValues()
        {
            ItemCollection coll = new ItemCollection();

            var cach = Container.Resolve<ICachedService>();
            lock (this)
                channels.ClearAndAddRange(cach.TelegramChannels.Items);

            foreach (var chn in channels.Items)
                coll.Add(chn.Id, chn.Name);

            return coll;
        }
    }

    public class MultilineTextBoxEditor : ITypeEditor
    {
        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            TextBox textBox = new TextBox
            {
                AcceptsReturn = true,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap
            };

            var binding = new Binding("Value")
            {
                Source = propertyItem,
                ValidatesOnExceptions = true,
                ValidatesOnDataErrors = true,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
            };

            BindingOperations.SetBinding(textBox, TextBox.TextProperty, binding);
            return textBox;
        }
    }
}