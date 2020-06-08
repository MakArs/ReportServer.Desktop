﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autofac;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using ItemCollection = Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection;

namespace ReportServer.Desktop.Views.WpfResources
{
    public class RecepGroupsSource : IItemsSource
    {
        public static IContainer Container;

        public ItemCollection GetValues()
        {
            ItemCollection coll = new ItemCollection();

            var cach = Container.Resolve<ICachedService>();

            var groups =
                new List<ApiRecepientGroup>();

            lock (this)
                groups.AddRange(cach.RecepientGroups.Items);

            coll.Add(0, "None");

            foreach (var rgr in groups)
                coll.Add(rgr.Id, rgr.Name);

            return coll;
        }
    }

    public class DelimitersSource : IItemsSource
    {
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

            foreach (var delim in delimitersList)
                coll.Add(delim.Value, delim.Name);

            return coll;
        }
    }

    public class TelegramChannelsSource : IItemsSource
    {
        public static IContainer Container;

        public ItemCollection GetValues()
        {
            ItemCollection coll = new ItemCollection();

            var channels =
                new List<ApiTelegramChannel>();
            var cach = Container.Resolve<ICachedService>();

            lock (this)
                channels.AddRange(cach.TelegramChannels.Items);

            foreach (var chn in channels)
                coll.Add(chn.Id, chn.Name);

            return coll;
        }
    }

    public class PathEditor : ReactiveObject, ITypeEditor
    {
        [Reactive] public PropertyItem PathValue { get; set; }
        [Reactive] public CheckBox CheckBoxIsDefault { get; set; }
        [Reactive] public TextBox PathTextBox { get; set; }

        public PathEditor()
        {
            this.WhenAnyValue(ped => ped.CheckBoxIsDefault.IsChecked)
                .Where(val => val != null)
                .Skip(1)
                .Subscribe(val =>
                    PathValue.Value = val == true ? "Default folder" : null);
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            PathValue = propertyItem;
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(3.5, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(2, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(0.5, GridUnitType.Star)}
                },
            };

            PathTextBox = new TextBox
            {
                IsEnabled = true,
                AcceptsReturn = false,
                TextAlignment = TextAlignment.Left,
                TextWrapping = TextWrapping.NoWrap,
            };
            Grid.SetColumn(PathTextBox, 0);
            grid.Children.Add(PathTextBox);

            TextBlock textBlockDefault = new TextBlock { Text = "Use default folder(for imported files)" };
            Grid.SetColumn(textBlockDefault, 1);
            grid.Children.Add(textBlockDefault);

            CheckBoxIsDefault = new CheckBox();
            Grid.SetColumn(CheckBoxIsDefault, 2);
            grid.Children.Add(CheckBoxIsDefault);

            var isDefaultBinding = new Binding("IsChecked")
            {
                Source = CheckBoxIsDefault,
                Converter = new InverseBoolConverter()
            };

            BindingOperations.SetBinding(PathTextBox, UIElement.IsEnabledProperty, isDefaultBinding);

            var textValueBinding =
                new Binding("Value")
                {
                    Source = PathValue,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
            BindingOperations.SetBinding(PathTextBox, TextBox.TextProperty, textValueBinding);

            return grid;
        }
    }

    public class PasswordEditor : ReactiveObject, ITypeEditor
    {
        [Reactive] public PropertyItem PathValue { get; set; }
        [Reactive] public PasswordBox PasswordTextBox { get; set; }
             
        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            PathValue = propertyItem;

            PasswordTextBox = new PasswordBox
            {
                IsEnabled = true,
            };
           
            PasswordTextBox.PasswordChanged += (sender, args) =>
            {
                PathValue.Value = PasswordTextBox.Password;
            };
            
            return PasswordTextBox;
        }
    }

    public class ClearIntervalEditor : ReactiveObject, ITypeEditor
    {
        [Reactive] public PropertyItem ClearInterval { get; set; }
        [Reactive] public CheckBox CheckBoxClear { get; set; }
        [Reactive] public TextBox ValueTextBox { get; set; }

        public ClearIntervalEditor()
        {
            this.WhenAnyValue(ped => ped.CheckBoxClear.IsChecked)
                .Where(val => val != null)
                .Skip(1)
                .Subscribe(val =>
                {
                    if (val == false)
                        ClearInterval.Value = 0;
                });
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            ClearInterval = propertyItem;
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(0.5, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(5.5, GridUnitType.Star)}
                },
            };

            ValueTextBox = new TextBox
            {
                IsEnabled = true,
                AcceptsReturn = false,
                TextAlignment = TextAlignment.Left,
                TextWrapping = TextWrapping.NoWrap,
            };
            Grid.SetColumn(ValueTextBox, 1);
            grid.Children.Add(ValueTextBox);
            
            CheckBoxClear = new CheckBox
            {
                IsChecked = (int)propertyItem.Value!=0
            };

            Grid.SetColumn(CheckBoxClear, 0);
            grid.Children.Add(CheckBoxClear);

            var clearBinding = new Binding("IsChecked")
            {
                Source = CheckBoxClear,
               // Converter = new InverseBoolConverter()
            };

            BindingOperations.SetBinding(ValueTextBox, UIElement.IsEnabledProperty, clearBinding);

            var textValueBinding =
                new Binding("Value")
                {
                    Source = ClearInterval,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
            BindingOperations.SetBinding(ValueTextBox, TextBox.TextProperty, textValueBinding);

            return grid;
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
