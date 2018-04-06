using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autofac;
using Gerakul.HttpUtils.Core;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using ReportServer.Desktop.ViewModel;
using Xceed.Wpf.Toolkit.Core.Converters;

namespace ReportServer.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BootsTrap.Init();
            var t = BootsTrap.Container.Resolve<IReportService>();
            var newtask=new ApiTask()
            {
                ConnectionString = null,Id = 12,ScheduleId = 3,Query = "weeklyreport_de",QueryTimeOut = 60,
                ViewTemplate = "weeklyreport_ve",RecepientGroupId = 1,TryCount = 5,
                TaskType = 2
            };
            t.DeleteTask(12);
            t.DeleteTask(13);
            t.DeleteTask(14);
            DataContext = BootsTrap.Container.Resolve<ICore>();
        }
    }
}
