using System.Windows;
using Autofac;
using AutoMapper;
using ReportServer.Desktop.Controls;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.ViewModel;

namespace ReportServer.Desktop.Windows
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
            var r = BootsTrap.Container.Resolve<IReportService>();
            var m= BootsTrap.Container.Resolve<IMapper>();
            //var reps=r.GetReports();
            DataContext = new Core(r,m);
        }
    }
}
