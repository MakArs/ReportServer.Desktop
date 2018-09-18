using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autofac;
using ReactiveUI;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using ItemCollection = Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection;

namespace ReportServer.Desktop.Views.WpfResources
{

    public class RecepGroupsSource : IItemsSource
    {
        private ReactiveList<ApiRecepientGroup> groups { get; set; }

        public static IContainer Container;

        public ItemCollection GetValues()
        {
            ItemCollection coll = new ItemCollection();

            var cach = Container.Resolve<ICachedService>();
            groups = cach.RecepientGroups;

            foreach (var rgr in groups)
                coll.Add(rgr.Id, rgr.Name);

            return coll;
        }
    }

    public class TelegramChannelsSource : IItemsSource
    {
        private ReactiveList<ApiTelegramChannel> channels { get; set; }
        public static IContainer Container;

        public ItemCollection GetValues()
        {
            ItemCollection coll = new ItemCollection();

            var cach = Container.Resolve<ICachedService>();
            channels = cach.TelegramChannels;

            foreach (var chn in channels)
                coll.Add(chn.Id, chn.Name);
            
            return coll;
        }
    }

    public class MultilineTextBoxEditor : ITypeEditor
    {
        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            TextBox textBox = new TextBox { AcceptsReturn = true,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap };

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
