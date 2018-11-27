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
        private SourceList<ApiRecepientGroup> groups { get; set; } =
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

    public class TelegramChannelsSource : IItemsSource
    {
        private SourceList<ApiTelegramChannel> channels { get; set; } =
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