using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using ComboBox = System.Windows.Controls.ComboBox;
using Control = System.Windows.Controls.Control;
using TextBox = System.Windows.Controls.TextBox;


namespace ReportServer.Desktop.Views.WpfResources
{
    public sealed class ComboboxSearcher : DependencyObject
    {
        public static void SetPropertyFilter(DependencyObject element, string propertyName)
        {
            if (element is Control controlSearch)
                controlSearch.KeyUp += (sender, e) =>
                {
                    if (sender is ComboBox control)
                    {
                        control.IsDropDownOpen = true;

                        var itemsViewOriginal =
                            (CollectionView) CollectionViewSource.GetDefaultView(control.ItemsSource);

                        var ihash = itemsViewOriginal.GetHashCode();
                        Debug.WriteLine(ihash);

                        var oldText = control.Text;

                        control.SelectedItem = null;

                        control.Text = oldText;


                        itemsViewOriginal.Filter = o =>
                        {
                            if (string.IsNullOrEmpty(oldText)) return true;
                            if (((string) o.GetType().GetProperty(propertyName)
                                    ?.GetValue(o, null))
                                .IndexOf(oldText, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                            return false;
                        };

                        var cmbTextBox = (TextBox) control.Template.FindName("PART_EditableTextBox", control);

                        cmbTextBox.SelectionLength = 0;
                        cmbTextBox.CaretIndex = cmbTextBox.Text.Length;
                    }
                };
        }
    }
}