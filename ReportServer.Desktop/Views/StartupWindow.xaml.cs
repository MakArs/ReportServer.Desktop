using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autofac;
using AutoMapper;
using MahApps.Metro.Controls;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.ViewModel;

namespace ReportServer.Desktop.Views
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : MetroWindow
    {
        public StartupWindow()
        {
            InitializeComponent();
            BootsTrap.Init();
            var r = BootsTrap.Container.Resolve<IReportService>();
            var m = BootsTrap.Container.Resolve<IMapper>();
            //var reps=r.GetReports();
            DataContext = new Core(r, m);
        }
    }
}
