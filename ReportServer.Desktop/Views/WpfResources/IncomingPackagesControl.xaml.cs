using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ReportServer.Desktop.Views.WpfResources
{
    public partial class IncomingPackagesControl: UserControl,ITypeEditor
    {
        public IncomingPackagesControl()
        {
            InitializeComponent();
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            return this;
        }
    }
}
