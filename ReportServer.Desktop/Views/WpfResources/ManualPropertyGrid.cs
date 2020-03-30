using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace ReportServer.Desktop.Views.WpfResources
{
    public class PropertyGridBehavior : Behavior<ContentControl>
    {
        private Dictionary<Type, Tuple<Type, DependencyProperty>> PropertyTemplates { get; set; }

        public PropertyGridBehavior()
        {
            PropertyTemplates = new Dictionary<Type, Tuple<Type, DependencyProperty>>
            {
                {
                    typeof(TextBoxProperty),
                    Tuple.Create(typeof(TextBox), TextBox.TextProperty)
                },

                {
                    typeof(ComboBoxProperty),
                    Tuple.Create(typeof(ComboBox), ItemsControl.ItemsSourceProperty)
                },

                {
                    typeof(CheckBoxProperty),
                    Tuple.Create(typeof(CheckBox), ToggleButton.IsCheckedProperty)
                }
            };
        }

        private ItemsControl container { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.DataContextChanged += AssociatedObjectDataContextChanged;
            AssociatedObject.Loaded += AssociatedObjectLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.DataContextChanged -= AssociatedObjectDataContextChanged;
            AssociatedObject.Loaded -= AssociatedObjectLoaded;
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            container=AssociatedObject.Template
                .FindName("ManualPropertyGrid",AssociatedObject) as ItemsControl;
        }

        private void AssociatedObjectDataContextChanged(object sender,
                                                        DependencyPropertyChangedEventArgs e)
        {
            if(container==null) return;
            container.Items.Clear();

            var props = e.NewValue.GetType().GetProperties();

            foreach (var prop in props)
            {
                foreach (var attr in prop.GetCustomAttributes())
                {
                    if(!PropertyTemplates.ContainsKey(attr.GetType()))
                        continue;

                    var templ = PropertyTemplates[attr.GetType()];

                    var element = Activator.CreateInstance(templ.Item1) as FrameworkElement;

                    var binding = new Binding
                    {
                        Path = new PropertyPath(prop.Name),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };

                    BindingOperations.SetBinding(element, templ.Item2, binding);

                    container.Items.Add(element);
                }
            }
        }
    }
    
    //<ContentControl Content = "{Binding Configuration}"
    //Grid.ColumnSpan="2"
    //Grid.Row="2"
    //Width="800"
    //DataContext="{Binding Configuration}">
    //<ContentControl.Template>
    //<ControlTemplate>
    //<ItemsControl x:Name="ManualPropertyGrid"/>
    //</ControlTemplate>
    //</ContentControl.Template>
    //<i:Interaction.Behaviors>
    //<wpfResources:PropertyGridBehavior/>
    //</i:Interaction.Behaviors>
    //</ContentControl>


    public class TextBoxProperty : Attribute
    {
    }

    public class ComboBoxProperty : Attribute
    {
    }

    public class CheckBoxProperty : Attribute
    {
    }

}
